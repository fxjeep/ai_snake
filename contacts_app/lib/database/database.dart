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
  BoolColumn get isPrinted => boolean().withDefault(const Constant(false))();
}

enum ContactType { Live, Dead, Ancestor, Property }

enum ImportStatus { imported, duplicate, skipped }

class ContactDetails extends Table {
  IntColumn get id => integer().autoIncrement()();
  IntColumn get contactId => integer().references(Contacts, #id)();
  BoolColumn get isPrinted => boolean().withDefault(const Constant(false))();
  DateTimeColumn get lastPrint => dateTime().nullable()();
  TextColumn get name1 => text()();
  TextColumn get name2 => text()();
  TextColumn get name3 => text()();
  TextColumn get type =>
      text().map(const EnumNameConverter<ContactType>(ContactType.values))();
}

@DriftDatabase(tables: [Contacts, ContactDetails])
class AppDatabase extends _$AppDatabase {
  final File dbFile;

  AppDatabase(this.dbFile) : super(NativeDatabase.createInBackground(dbFile));

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
    return (select(
      contacts,
    )..where((t) => t.name.like('%$query%') | t.code.like('%$query%'))).watch();
  }

  // Search contacts by name or code (Future for autocomplete)
  Future<List<Contact>> searchContactsFuture(String query) {
    return (select(
      contacts,
    )..where((t) => t.name.like('%$query%') | t.code.like('%$query%'))).get();
  }

  // CRUD operations
  Future<int> addContact(ContactsCompanion entry) =>
      into(contacts).insert(entry);
  Future<bool> updateContact(Contact entry) => update(contacts).replace(entry);
  Future<int> deleteContact(Contact entry) => delete(contacts).delete(entry);

  // Detail operations
  Stream<List<ContactDetail>> watchDetailsForContact(int contactId) {
    return (select(
      contactDetails,
    )..where((t) => t.contactId.equals(contactId))).watch();
  }

  Stream<List<ContactDetail>> watchDetailsByType(
    int contactId,
    ContactType type, {
    String? query,
  }) {
    return (select(contactDetails)..where((t) {
          final matchesContactAndType =
              t.contactId.equals(contactId) & t.type.equalsValue(type);
          if (query == null || query.isEmpty) {
            return matchesContactAndType;
          }
          return matchesContactAndType &
              (t.name1.like('%$query%') |
                  t.name2.like('%$query%') |
                  t.name3.like('%$query%'));
        }))
        .watch();
  }

  Future<int> addDetail(ContactDetailsCompanion entry) =>
      into(contactDetails).insert(entry);
  Future<bool> updateDetail(ContactDetail entry) =>
      update(contactDetails).replace(entry);
  Future<int> deleteDetail(ContactDetail entry) =>
      delete(contactDetails).delete(entry);

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
      ContactDetailsCompanion(lastPrint: Value(lastPrint)),
    );
  }

  Future<void> clearAllPrintStatus() async {
    await transaction(() async {
      await update(
        contacts,
      ).write(const ContactsCompanion(isPrinted: Value(false)));
      await update(
        contactDetails,
      ).write(const ContactDetailsCompanion(isPrinted: Value(false)));
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

  Future<void> mergeContacts(int sourceId, int targetId) async {
    await transaction(() async {
      // 1. Move all details from source to target
      await (update(contactDetails)..where((t) => t.contactId.equals(sourceId)))
          .write(ContactDetailsCompanion(contactId: Value(targetId)));

      // 2. Delete the source contact
      await (delete(contacts)..where((t) => t.id.equals(sourceId))).go();
    });
  }

  Future<ImportStatus> importSingleRecord(ImportRecord row) async {
    // 1. Find or create contact
    var contact = await (select(contacts)
          ..where(
            (t) =>
                t.name.equals(row.contactName) & t.code.equals(row.contactCode),
          )
          ..limit(1))
        .getSingleOrNull();

    int contactId;
    if (contact == null) {
      contactId = await into(contacts).insert(
        ContactsCompanion.insert(
          name: row.contactName,
          code: row.contactCode,
          lastPrintDate: Value(row.contactLastPrintDate),
          isPrinted: const Value(false),
        ),
      );
    } else {
      contactId = contact.id;
    }

    // 2. Map type string to enum
    ContactType type;
    switch (row.typeStr.toLowerCase()) {
      case 'dead':
        type = ContactType.Dead;
        break;
      case 'ancestor':
        type = ContactType.Ancestor;
        break;
      case 'property':
        type = ContactType.Property;
        break;
      case 'live':
      default:
        type = ContactType.Live;
        break;
    }

    // 3. Check for duplicates
    final existingDetail = await (select(contactDetails)
          ..where(
            (t) =>
                t.contactId.equals(contactId) &
                t.type.equalsValue(type) &
                t.name1.equals(row.name1) &
                t.name2.equals(row.name2) &
                t.name3.equals(row.name3),
          )
          ..limit(1))
        .getSingleOrNull();

    if (existingDetail == null) {
      // 4. Insert detail record
      await into(contactDetails).insert(
        ContactDetailsCompanion.insert(
          contactId: contactId,
          name1: row.name1,
          name2: row.name2,
          name3: row.name3,
          type: type,
          lastPrint: Value(row.detailLastPrintDate),
          isPrinted: const Value(false),
        ),
      );
      return ImportStatus.imported;
    } else {
      return ImportStatus.duplicate;
    }
  }

  Future<ImportStats> importRecordsBatch(List<ImportRecord> rows) async {
    int imported = 0;
    int duplicates = 0;
    await transaction(() async {
      for (final row in rows) {
        final status = await importSingleRecord(row);
        if (status == ImportStatus.imported) {
          imported++;
        } else if (status == ImportStatus.duplicate) {
          duplicates++;
        }
      }
    });
    return ImportStats(imported: imported, duplicates: duplicates);
  }

  Future<void> importRecords(List<ImportRecord> rows) async {
    await importRecordsBatch(rows);
  }

  Future<List<Contact>> getContactsByPrintDateAfter(DateTime date) async {
    return await (select(contacts)
          ..where((t) => t.lastPrintDate.isBiggerThanValue(date))
          ..orderBy([(t) => OrderingTerm(expression: t.name)]))
        .get();
  }

  Future<Contact?> findContact(String query) async {
    return await (select(contacts)
          ..where((t) => t.name.equals(query) | t.code.equals(query))
          ..limit(1))
        .getSingleOrNull();
  }

  Future<Contact?> findContactByNameAndCode(String name, String code) async {
    return await (select(contacts)
          ..where((t) => t.name.equals(name) & t.code.equals(code))
          ..limit(1))
        .getSingleOrNull();
  }

  Stream<Contact> watchContactById(int id) {
    return (select(contacts)..where((t) => t.id.equals(id))).watchSingle();
  }
}

class ImportRecord {
  final String contactName;
  final String contactCode;
  final DateTime? contactLastPrintDate;
  final String typeStr;
  final String name1;
  final String name2;
  final String name3;
  final DateTime? detailLastPrintDate;

  ImportRecord({
    required this.contactName,
    required this.contactCode,
    this.contactLastPrintDate,
    required this.typeStr,
    required this.name1,
    required this.name2,
    required this.name3,
    this.detailLastPrintDate,
  });
}

class ContactPrintData {
  final Contact contact;
  final List<ContactDetail> details;

  ContactPrintData({required this.contact, required this.details});
}

class ImportStats {
  final int imported;
  final int duplicates;

  ImportStats({required this.imported, required this.duplicates});
}
