import 'dart:io';

import 'package:drift/drift.dart';
import 'package:drift/native.dart';
import 'package:path_provider/path_provider.dart';
import 'package:path/path.dart' as p;

part 'database.g.dart';

class Contacts extends Table {
  IntColumn get id => integer().autoIncrement()();
  TextColumn get name => text()();
  TextColumn get code => text()();
  DateTimeColumn get lastPrintDate => dateTime().nullable()();
  TextColumn get initials => text()();
  TextColumn get profileImage => text().nullable()();
  BoolColumn get isPrinted => boolean().withDefault(const Constant(false))();
}

enum ContactType { Live, Dead, Ancestor, Property }

class ContactDetails extends Table {
  IntColumn get id => integer().autoIncrement()();
  IntColumn get contactId => integer().references(Contacts, #id)();
  BoolColumn get isPrinted => boolean().withDefault(const Constant(false))();
  DateTimeColumn get lastPrint => dateTime().nullable()();
  TextColumn get name1 => text()();
  TextColumn get name2 => text()();
  TextColumn get name3 => text()();
  TextColumn get type => text().map(const EnumNameConverter<ContactType>(ContactType.values))();
}

@DriftDatabase(tables: [Contacts, ContactDetails])
class AppDatabase extends _$AppDatabase {
  AppDatabase() : super(_openConnection());

  @override
  int get schemaVersion => 3;

  @override
  MigrationStrategy get migration {
    return MigrationStrategy(
      onCreate: (m) async {
        await m.createAll();
      },
      onUpgrade: (m, from, to) async {
        if (from < 2) {
          await m.createTable(contactDetails);
        }
        if (from < 3) {
          await m.addColumn(contacts, contacts.isPrinted);
        }
      },
    );
  }

  // Watch all contacts, sorted by name
  Stream<List<Contact>> watchAllContacts() => select(contacts).watch();

  // Search contacts by name or code
  Stream<List<Contact>> searchContacts(String query) {
    return (select(contacts)
          ..where((t) => t.name.like('%$query%') | t.code.like('%$query%')))
        .watch();
  }

  // CRUD operations
  Future<int> addContact(ContactsCompanion entry) => into(contacts).insert(entry);
  Future<bool> updateContact(Contact entry) => update(contacts).replace(entry);
  Future<int> deleteContact(Contact entry) => delete(contacts).delete(entry);

  // Detail operations
  Stream<List<ContactDetail>> watchDetailsForContact(int contactId) {
    return (select(contactDetails)..where((t) => t.contactId.equals(contactId))).watch();
  }

  Stream<List<ContactDetail>> watchDetailsByType(int contactId, ContactType type, {String? query}) {
    return (select(contactDetails)
          ..where((t) {
            final matchesContactAndType = t.contactId.equals(contactId) & t.type.equalsValue(type);
            if (query == null || query.isEmpty) {
              return matchesContactAndType;
            }
            return matchesContactAndType &
                (t.name1.like('%$query%') | t.name2.like('%$query%') | t.name3.like('%$query%'));
          }))
        .watch();
  }

  Future<int> addDetail(ContactDetailsCompanion entry) => into(contactDetails).insert(entry);
  Future<bool> updateDetail(ContactDetail entry) => update(contactDetails).replace(entry);
  Future<int> deleteDetail(ContactDetail entry) => delete(contactDetails).delete(entry);

  Future<void> batchUpdatePrintStatus(Set<int> ids, bool isPrinted) async {
    await (update(contactDetails)..where((t) => t.id.isIn(ids))).write(
      ContactDetailsCompanion(
        isPrinted: Value(isPrinted),
        // Removed lastPrint update - only setting isPrinted
      ),
    );
  }

  Future<void> batchDeleteDetails(Set<int> ids) async {
    await (delete(contactDetails)..where((t) => t.id.isIn(ids))).go();
  }

  Future<void> batchUpdateLastPrint(Set<int> ids, DateTime lastPrint) async {
    await (update(contactDetails)..where((t) => t.id.isIn(ids))).write(
      ContactDetailsCompanion(
        lastPrint: Value(lastPrint),
      ),
    );
  }

  Future<void> clearAllPrintStatus() async {
    await transaction(() async {
      await update(contacts).write(
        const ContactsCompanion(isPrinted: Value(false)),
      );
      await update(contactDetails).write(
        const ContactDetailsCompanion(isPrinted: Value(false)),
      );
    });
  }

  // Aggregate query for PrintView
  Stream<List<ContactPrintData>> watchContactsWithDetails() {
    final query = select(contacts).join([
      leftOuterJoin(
        contactDetails,
        contactDetails.contactId.equalsExp(contacts.id),
      ),
    ]);

    return query.watch().map((rows) {
      final groupedData = <int, ContactPrintData>{};

      for (final row in rows) {
        final contact = row.readTable(contacts);
        final detail = row.readTableOrNull(contactDetails);

        final data = groupedData.putIfAbsent(
          contact.id,
          () => ContactPrintData(contact: contact, details: []),
        );

        if (detail != null) {
          data.details.add(detail);
        }
      }

      return groupedData.values.where((data) {
        final hasPrintedDetails = data.details.any((d) => d.isPrinted);
        return data.contact.isPrinted || hasPrintedDetails;
      }).toList();
    });
  }

  Stream<Contact> watchContactById(int id) {
    return (select(contacts)..where((t) => t.id.equals(id))).watchSingle();
  }
}

class ContactPrintData {
  final Contact contact;
  final List<ContactDetail> details;

  ContactPrintData({required this.contact, required this.details});
}

LazyDatabase _openConnection() {
  return LazyDatabase(() async {
    final dbFolder = await getApplicationDocumentsDirectory();
    final file = File(p.join(dbFolder.path, 'db.sqlite'));
    return NativeDatabase.createInBackground(file);
  });
}
