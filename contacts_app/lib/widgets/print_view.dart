import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../database/database.dart';
import '../core/theme.dart';
import 'detail_editor.dart';

class PrintView extends StatelessWidget {
  const PrintView({super.key});

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
                IconButton(
                  icon: const Icon(Icons.refresh, color: AppTheme.primaryColor),
                  onPressed: () {
                    // Re-triggering build is enough for the date and StreamBuilder recreation
                    (context as Element).markNeedsBuild();
                  },
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
