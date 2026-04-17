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
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _yearController.text = DateTime.now().year.toString();
  }

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
    if (result == null || !mounted) return;

    final path = result.files.single.path;
    if (path == null) return;

    final db = Provider.of<AppDatabase>(context, listen: false);

    // Initial state for progress tracking
    int totalLines = 0;
    int processedCount = 0;
    int importedCount = 0;
    int ignoredCount = 0; // Duplicates and malformed
    bool isCancelled = false;
    bool isDone = false;
    String? errorMessage;

    // Show progress dialog
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext dialogContext) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            // Inner function to perform the import logic
            Future<void> startImport() async {
              try {
                final file = File(path);
                
                // 1. Count total lines (ignoring empty lines)
                // This gives us the "total records" for the progress bar
                final countStream = file.openRead()
                    .transform(utf8.decoder)
                    .transform(const LineSplitter());
                
                int count = 0;
                await for (final line in countStream) {
                  if (line.trim().isNotEmpty) count++;
                }
                
                if (!mounted) return;
                setDialogState(() {
                  totalLines = count;
                });

                if (totalLines == 0) {
                  isDone = true;
                  setDialogState(() {});
                  return;
                }

                // 2. Process records in batches for performance
                final lineStream = file.openRead()
                    .transform(utf8.decoder)
                    .transform(const LineSplitter());

                List<ImportRecord> batch = [];
                const int batchSize = 50;

                await for (final line in lineStream) {
                  if (isCancelled) break;

                  if (line.trim().isEmpty) continue;

                  try {
                    final fields = line.split(',');
                    if (fields.length < 8) {
                      ignoredCount++;
                    } else {
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

                      batch.add(
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
                  } catch (e) {
                    ignoredCount++;
                  }

                  if (batch.length >= batchSize) {
                    final stats = await db.importRecordsBatch(batch);
                    importedCount += stats.imported;
                    ignoredCount += stats.duplicates;
                    processedCount += batch.length;
                    batch.clear();
                    setDialogState(() {});
                  }
                }

                // Process remaining records in the last batch
                if (!isCancelled && batch.isNotEmpty) {
                  final stats = await db.importRecordsBatch(batch);
                  importedCount += stats.imported;
                  ignoredCount += stats.duplicates;
                  processedCount += batch.length;
                  batch.clear();
                } else if (isCancelled) {
                   // If cancelled, processedCount might not match totalLines
                   // but we show what we got.
                }

              } catch (e) {
                errorMessage = e.toString();
              } finally {
                isDone = true;
                if (mounted) setDialogState(() {});
              }
            }

            // Start import on first build of the dialog
            // We use a flag to ensure it only starts once
            if (totalLines == 0 && !isDone && errorMessage == null) {
               startImport();
            }

            return AlertDialog(
              backgroundColor: AppTheme.surfaceColor,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              title: Text(
                isDone ? (errorMessage != null ? 'Import Error' : 'Import Complete') : 'Importing Records...',
                style: const TextStyle(color: Colors.white),
              ),
              content: SizedBox(
                width: 300,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    if (!isDone && totalLines == 0) ...[
                       const CircularProgressIndicator(color: AppTheme.primaryColor),
                       const SizedBox(height: 16),
                       const Text('Analyzing file...', style: TextStyle(color: Colors.white70)),
                    ] else if (errorMessage != null) ...[
                       Text('Error: $errorMessage', style: const TextStyle(color: Colors.redAccent)),
                    ] else ...[
                      LinearProgressIndicator(
                        value: totalLines > 0 ? processedCount / totalLines : 0,
                        backgroundColor: Colors.white12,
                        color: AppTheme.primaryColor,
                      ),
                      const SizedBox(height: 16),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            'Processed: $processedCount / $totalLines',
                            style: const TextStyle(color: Colors.white70, fontSize: 13),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            'Imported: $importedCount',
                            style: const TextStyle(color: Colors.greenAccent, fontSize: 13),
                          ),
                          Text(
                            'Ignored: $ignoredCount',
                            style: const TextStyle(color: Colors.orangeAccent, fontSize: 13),
                          ),
                        ],
                      ),
                    ],
                  ],
                ),
              ),
              actions: [
                if (!isDone)
                  TextButton(
                    onPressed: () {
                      isCancelled = true;
                      setDialogState(() {});
                    },
                    child: const Text('Cancel', style: TextStyle(color: Colors.redAccent)),
                  )
                else
                  TextButton(
                    onPressed: () => Navigator.of(dialogContext).pop(),
                    child: const Text('Close', style: TextStyle(color: AppTheme.primaryColor)),
                  ),
              ],
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        SafeArea(
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
                              child: DropdownButtonHideUnderline(
                                child: DropdownButton<String>(
                                  value: _yearController.text,
                                  isExpanded: true,
                                  dropdownColor: const Color(0xFF1c2732),
                                  icon: const Icon(
                                    Icons.arrow_drop_down,
                                    color: AppTheme.secondaryTextColor,
                                  ),
                                  style: const TextStyle(
                                    color: Colors.white,
                                    fontSize: 14,
                                  ),
                                  onChanged: (String? newValue) {
                                    if (newValue != null) {
                                      setState(() {
                                        _yearController.text = newValue;
                                      });
                                    }
                                  },
                                  items: List.generate(10, (index) {
                                    final year = DateTime.now().year - index;
                                    return DropdownMenuItem<String>(
                                      value: year.toString(),
                                      child: Text(year.toString()),
                                    );
                                  }),
                                ),
                              ),
                            ),
                            const SizedBox(height: 16),
                            // Extract Button
                            SizedBox(
                              width: double.infinity,
                              height: 40,
                              child: ElevatedButton.icon(
                                onPressed: _isLoading ? null : _extractContactList,
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
                                onPressed:
                                    _isLoading ? null : _importDetailRecords,
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
        ),
        if (_isLoading)
          const Opacity(
            opacity: 0.5,
            child: ModalBarrier(dismissible: false, color: Colors.black),
          ),
        if (_isLoading)
          const Center(
            child: CircularProgressIndicator(color: AppTheme.primaryColor),
          ),
      ],
    );
  }
}
