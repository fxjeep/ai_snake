import 'dart:io';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import 'package:file_picker/file_picker.dart';
import 'package:drift/drift.dart' show Value;
import '../database/database.dart';
import '../core/theme.dart';
import 'detail_editor.dart';

class PrintView extends StatefulWidget {
  const PrintView({super.key});

  @override
  State<PrintView> createState() => _PrintViewState();
}

class _PrintViewState extends State<PrintView> {
  bool _isGenerating = false;

  Future<void> _generatePrintFile(BuildContext context, AppDatabase db) async {
    if (_isGenerating) return;
    setState(() => _isGenerating = true);
    try {
      // 1. Get save location
      String? outputFile = await FilePicker.platform.saveFile(
        dialogTitle: 'Save Print File',
        fileName: 'print_${DateFormat('yyyyMMdd').format(DateTime.now())}.txt',
        type: FileType.custom,
        allowedExtensions: ['txt'],
      );

      if (outputFile == null) return;

      // 2. Fetch all data
      final List<ContactPrintData> allData = await db.watchContactsWithDetails().first;
      
      final buffer = StringBuffer();

      // 3. Format data
      for (final data in allData) {
        final contact = data.contact;
        final details = data.details;

        // Determination of which details to export
        List<ContactDetail> detailsToExport;
        if (contact.isPrinted) {
          // Master is ON: export all details
          detailsToExport = details;
        } else {
          // Master is OFF: export only details with isPrinted == true
          detailsToExport = details.where((d) => d.isPrinted).toList();
        }

        for (final detail in detailsToExport) {
          final fields = [
            contact.code,
            contact.name,
            _getTypeFullText(detail.type),
            detail.name1,
            detail.name2 ?? '',
            detail.name3 ?? '',
          ];
          
          buffer.write(fields.join('\t'));
          buffer.write('\r\n');
        }
      }

      // 4. Save file
      final file = File(outputFile);
      await file.writeAsString(buffer.toString());

      // 5. Update lastPrint timestamps
      final now = DateTime.now();

      // Collect detail IDs to update and track which contacts to update
      final Set<int> detailIdsToUpdate = {};
      final Set<int> contactIdsToUpdate = {};

      for (final data in allData) {
        final contact = data.contact;
        final details = data.details;

        List<ContactDetail> detailsToExport;
        if (contact.isPrinted) {
          detailsToExport = details;
        } else {
          detailsToExport = details.where((d) => d.isPrinted).toList();
        }

        if (detailsToExport.isNotEmpty) {
          contactIdsToUpdate.add(contact.id);
          for (final d in detailsToExport) {
            if (d.isPrinted) detailIdsToUpdate.add(d.id);
          }
        }
      }

      // Batch update detail lastPrint
      if (detailIdsToUpdate.isNotEmpty) {
        await db.batchUpdateLastPrint(detailIdsToUpdate, now);
      }

      // Update master contact lastPrintDate
      for (final data in allData) {
        if (contactIdsToUpdate.contains(data.contact.id)) {
          await db.updateContact(
            data.contact.copyWith(lastPrintDate: Value(now)),
          );
        }
      }

      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('File saved to $outputFile')),
        );
      }
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error generating file: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _isGenerating = false);
    }
  }

  String _getTypeFullText(ContactType type) {
    switch (type) {
      case ContactType.Live:
        return 'Live';
      case ContactType.Dead:
        return 'Dead';
      case ContactType.Ancestor:
        return 'Ancestor';
      case ContactType.Property:
        return 'Property';
    }
  }

  @override
  Widget build(BuildContext context) {
    final db = Provider.of<AppDatabase>(context);
    final today = DateFormat('yyyy-MM-dd').format(DateTime.now());

    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 20),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Current Print at $today',
                  style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                    color: Colors.white,
                  ),
                ),
                Row(
                  children: [
                    _HeaderButton(
                      icon: Icons.auto_awesome,
                      label: 'Generate',
                      onPressed: _isGenerating ? null : () => _generatePrintFile(context, db),
                    ),
                    const SizedBox(width: 12),
                    _HeaderButton(
                      icon: Icons.clear_all,
                      label: 'Clear',
                      onPressed: () async {
                        final confirm = await showDialog<bool>(
                          context: context,
                          builder: (context) => AlertDialog(
                            backgroundColor: const Color(0xFF1c2732),
                            title: const Text(
                              'Clear All Print Status',
                              style: TextStyle(color: Colors.white),
                            ),
                            content: const Text(
                              'This will reset the print status for all contacts and their records. This cannot be undone. Continue?',
                              style: TextStyle(color: Colors.white70),
                            ),
                            actions: [
                              TextButton(
                                onPressed: () => Navigator.pop(context, false),
                                child: const Text('Cancel', style: TextStyle(color: Colors.white54)),
                              ),
                              TextButton(
                                onPressed: () => Navigator.pop(context, true),
                                child: const Text('Clear All', style: TextStyle(color: Colors.redAccent)),
                              ),
                            ],
                          ),
                        );
                        if (confirm == true && context.mounted) {
                          await db.clearAllPrintStatus();
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('All print statuses cleared')),
                          );
                        }
                      },
                    ),
                    const SizedBox(width: 8),
                    IconButton(
                      icon: const Icon(Icons.refresh, color: AppTheme.primaryColor),
                      onPressed: () {
                        (context as Element).markNeedsBuild();
                      },
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 20),
            Expanded(
              child: StreamBuilder<List<ContactPrintData>>(
                stream: db.watchContactsWithDetails(),
                builder: (context, snapshot) {
                  if (!snapshot.hasData) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  final data = snapshot.data!;
                  if (data.isEmpty) {
                    return const Center(
                      child: Text(
                        'No records to print',
                        style: TextStyle(color: AppTheme.secondaryTextColor),
                      ),
                    );
                  }

                  final sortedData = List<ContactPrintData>.from(data)
                    ..sort((a, b) => a.contact.name.compareTo(b.contact.name));

                  return Column(
                    children: [
                      // Table Header
                      Padding(
                        padding: const EdgeInsets.symmetric(vertical: 12.0, horizontal: 8.0),
                        child: Row(
                          children: const [
                            Expanded(
                              flex: 3,
                              child: Text(
                                'CONTACT',
                                style: TextStyle(
                                  color: AppTheme.secondaryTextColor,
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            Expanded(
                              flex: 2,
                              child: Text(
                                'COUNTS',
                                textAlign: TextAlign.center,
                                style: TextStyle(
                                  color: AppTheme.secondaryTextColor,
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            Expanded(
                              flex: 1,
                              child: Text(
                                'ACTION',
                                textAlign: TextAlign.right,
                                style: TextStyle(
                                  color: AppTheme.secondaryTextColor,
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const Divider(color: Colors.white10, height: 1),
                      Expanded(
                        child: ListView.builder(
                          itemCount: sortedData.length,
                          itemBuilder: (context, index) {
                            final item = sortedData[index];
                            return _PrintRow(item: item);
                          },
                        ),
                      ),
                    ],
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _PrintRow extends StatelessWidget {
  final ContactPrintData item;

  const _PrintRow({required this.item});

  String _formatCounts() {
    final typeStats = <ContactType, (int printed, int total)>{};
    
    // Initialize stats for all types
    for (var type in ContactType.values) {
      typeStats[type] = (0, 0);
    }

    // Aggregate counts
    for (var detail in item.details) {
      final current = typeStats[detail.type]!;
      final isPrintedValue = item.contact.isPrinted || detail.isPrinted;
      typeStats[detail.type] = (
        current.$1 + (isPrintedValue ? 1 : 0),
        current.$2 + 1
      );
    }

    // Build formatted string
    final parts = <String>[];
    final prefixes = {
      ContactType.Live: 'L',
      ContactType.Dead: 'D',
      ContactType.Ancestor: 'A',
      ContactType.Property: 'P',
    };

    for (var type in ContactType.values) {
      final stats = typeStats[type]!;
      if (stats.$2 > 0) {
        parts.add('${prefixes[type]}:${stats.$1}/${stats.$2}');
      }
    }

    return parts.join(' ');
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: Colors.white10)),
      ),
      child: Row(
        children: [
          Expanded(
            flex: 3,
            child: Row(
              children: [
                Text(
                  item.contact.name,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(width: 8),
                Text(
                  item.contact.code,
                  style: const TextStyle(
                    color: AppTheme.primaryColor,
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            flex: 2,
            child: Text(
              _formatCounts(),
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 14,
                fontFamily: 'monospace',
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          Expanded(
            flex: 1,
            child: Align(
              alignment: Alignment.centerRight,
              child: IconButton(
                icon: const Icon(Icons.edit, color: AppTheme.primaryColor, size: 20),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => DetailEditor(contact: item.contact),
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _HeaderButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback? onPressed;

  const _HeaderButton({
    required this.icon,
    required this.label,
    this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return TextButton.icon(
      onPressed: onPressed,
      icon: Icon(icon, size: 18, color: AppTheme.primaryColor),
      label: Text(
        label,
        style: const TextStyle(
          color: AppTheme.primaryColor,
          fontWeight: FontWeight.bold,
          fontSize: 14,
        ),
      ),
      style: TextButton.styleFrom(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        backgroundColor: AppTheme.primaryColor.withOpacity(0.1),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
        ),
      ),
    );
  }
}
