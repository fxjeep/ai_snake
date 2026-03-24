import 'dart:io';
import 'package:flutter/material.dart';
import 'package:path_provider/path_provider.dart';
import 'package:path/path.dart' as p;
import 'database.dart';

class DatabaseProvider extends ChangeNotifier {
  AppDatabase? _database;
  String? _currentPath;

  AppDatabase get database {
    if (_database == null) {
      throw StateError('Database not initialized. Call initialize() first.');
    }
    return _database!;
  }

  String get currentPath => _currentPath ?? '';

  Future<void> initialize() async {
    if (_database != null) return;

    final dbFolder = await getApplicationDocumentsDirectory();
    final defaultPath = p.join(dbFolder.path, 'db.sqlite');
    _currentPath = defaultPath;
    _database = AppDatabase(File(defaultPath));
    notifyListeners();
  }

  Future<void> setDatabasePath(String newPath) async {
    if (_database != null) {
      await _database!.close();
    }

    _currentPath = newPath;
    _database = AppDatabase(File(newPath));
    notifyListeners();
  }

  @override
  void dispose() {
    _database?.close();
    super.dispose();
  }
}
