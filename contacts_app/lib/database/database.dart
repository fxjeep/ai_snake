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

@DriftDatabase(tables: [Contacts])
class AppDatabase extends _$AppDatabase {
  AppDatabase() : super(_openConnection());

  @override
  int get schemaVersion => 1;

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
}

LazyDatabase _openConnection() {
  return LazyDatabase(() async {
    final dbFolder = await getApplicationDocumentsDirectory();
    final file = File(p.join(dbFolder.path, 'db.sqlite'));
    return NativeDatabase.createInBackground(file);
  });
}
