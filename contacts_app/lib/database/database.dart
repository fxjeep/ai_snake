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
  int get schemaVersion => 2;

  @override
  MigrationStrategy get migration {
    return MigrationStrategy(
      onCreate: (m) async {
        await m.createAll();
      },
      onUpgrade: (m, from, to) async {
        if (from < 2) {
          // Add the contactDetails table
          await m.createTable(contactDetails);
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
}

LazyDatabase _openConnection() {
  return LazyDatabase(() async {
    final dbFolder = await getApplicationDocumentsDirectory();
    final file = File(p.join(dbFolder.path, 'db.sqlite'));
    return NativeDatabase.createInBackground(file);
  });
}
