import 'dart:io';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:provider/provider.dart';
import 'package:drift/drift.dart' hide Column;
import '../database/database.dart';
import '../core/theme.dart';

class DataView extends StatefulWidget {
  const DataView({super.key});

  @override
  State<DataView> createState() => _DataViewState();
}

class _DataViewState extends State<DataView> {
  final TextEditingController _yearController = TextEditingController();

  @override
  void dispose() {
    _yearController.dispose();
    super.dispose();
  }

  Future<void> _extractContactList() async {
    final yearStr = _yearController.text.trim();
    if (yearStr.length != 4 || int.tryParse(yearStr) == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter a valid 4-digit year')),
      );
      return;
    }

    final year = int.parse(yearStr);
    final db = Provider.of<AppDatabase>(context, listen: false);
    final cutoffDate = DateTime(year, 1, 1);

    final contactsList = await db.getContactsByPrintDateAfter(cutoffDate);

    if (contactsList.isEmpty) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('No contacts found for the selected year'),
          ),
        );
      }
      return;
    }

    final path = await FilePicker.platform.saveFile(
      dialogTitle: 'Save Contact List',
      fileName: 'contact_list_$year.csv',
      type: FileType.custom,
      allowedExtensions: ['csv'],
    );

    if (path != null && mounted) {
      try {
        final List<String> csvLines = [];
        // Header
        csvLines.add('Contact Name,Contact Code,Last Print Date');

        for (final contact in contactsList) {
          final dateStr =
              contact.lastPrintDate?.toIso8601String().substring(0, 10) ?? '';
          csvLines.add('"${contact.name}","${contact.code}",$dateStr');
        }

        final file = File(path);
        await file.writeAsString(csvLines.join('\n'));

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(
                'Extracted ${contactsList.length} contacts to $path',
              ),
            ),
          );
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(SnackBar(content: Text('Error saving file: $e')));
        }
      }
    }
  }

  Future<void> _importDetailRecords() async {
    final result = await FilePicker.platform.pickFiles(
      dialogTitle: 'Select Import File',
      type: FileType.custom,
      allowedExtensions: ['txt', 'csv'],
    );
    if (result != null && mounted) {
      final path = result.files.single.path;
      if (path == null) return;

      final db = Provider.of<AppDatabase>(context, listen: false);

      try {
        final file = File(path);
        final lines = await file.readAsLines();
        final List<ImportRecord> importRows = [];

        for (final line in lines) {
          if (line.trim().isEmpty) continue;

          final fields = line.split('\t');
          if (fields.length < 8) continue;

          final contactName = fields[0].trim();
          final contactCode = fields[1].trim();
          final contactLastPrintStr = fields[2].trim();
          final typeStr = fields[3].trim();
          final name1 = fields[4].trim();
          final name2 = fields[5].trim();
          final name3 = fields[6].trim();
          final detailLastPrintStr = fields[7].trim();

          final contactLastPrint = DateTime.tryParse(contactLastPrintStr);
          final detailLastPrint = DateTime.tryParse(detailLastPrintStr);

          importRows.add(
            ImportRecord(
              contactName: contactName,
              contactCode: contactCode,
              contactLastPrintDate: contactLastPrint,
              typeStr: typeStr,
              name1: name1,
              name2: name2,
              name3: name3,
              detailLastPrintDate: detailLastPrint,
            ),
          );
        }

        if (importRows.isNotEmpty) {
          await db.importRecords(importRows);
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(
                  'Imported ${importRows.length} records successfully',
                ),
              ),
            );
          }
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(SnackBar(content: Text('Error during import: $e')));
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 20),
            const Text(
              'Data',
              style: TextStyle(
                fontSize: 34,
                fontWeight: FontWeight.bold,
                color: Colors.white,
              ),
            ),
            const SizedBox(height: 20),

            // Side-by-Side Panels
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Extract Panel
                Expanded(
                  child: Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppTheme.surfaceColor,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.white10),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Extract Contact List',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 16),
                        // Year Input
                        const Text(
                          'Last print since year. eg. 2020',
                          style: TextStyle(
                            color: AppTheme.secondaryTextColor,
                            fontSize: 12,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Container(
                          height: 40,
                          decoration: BoxDecoration(
                            color: const Color(0xFF1c2732),
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: Colors.white12),
                          ),
                          padding: const EdgeInsets.symmetric(horizontal: 12),
                          child: TextField(
                            controller: _yearController,
                            keyboardType: TextInputType.number,
                            maxLength: 4,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 14,
                            ),
                            decoration: const InputDecoration(
                              hintText: 'e.g. 2020',
                              hintStyle: TextStyle(
                                color: AppTheme.secondaryTextColor,
                              ),
                              border: InputBorder.none,
                              counterText: '',
                              contentPadding: EdgeInsets.only(bottom: 11),
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),
                        // Extract Button
                        SizedBox(
                          width: double.infinity,
                          height: 40,
                          child: ElevatedButton.icon(
                            onPressed: _extractContactList,
                            icon: const Icon(Icons.download, size: 18),
                            label: const Text('Extract'),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: AppTheme.primaryColor,
                              foregroundColor: Colors.white,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(width: 16),
                // Import Panel
                Expanded(
                  child: Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppTheme.surfaceColor,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.white10),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Import',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 16),
                        const Text(
                          'Import detail records',
                          style: TextStyle(
                            color: AppTheme.secondaryTextColor,
                            fontSize: 12,
                          ),
                        ),
                        const SizedBox(height: 6),
                        // Spacer to align with TextField height in the left column
                        const SizedBox(height: 40),
                        const SizedBox(height: 16),
                        // File Pick Button
                        SizedBox(
                          width: double.infinity,
                          height: 40,
                          child: ElevatedButton.icon(
                            onPressed: _importDetailRecords,
                            icon: const Icon(Icons.upload_file, size: 18),
                            label: const Text('Choose File'),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: const Color(0xFF1c2732),
                              foregroundColor: AppTheme.primaryColor,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8),
                                side: const BorderSide(
                                  color: AppTheme.primaryColor,
                                ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),

            const SizedBox(height: 20),

            // Placeholder for future data table
            const Expanded(
              child: Center(
                child: Text(
                  'Data table coming soon',
                  style: TextStyle(color: AppTheme.secondaryTextColor),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
