import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../database/database.dart';
import '../core/theme.dart';
import '../widgets/contact_list_tile.dart';
import '../widgets/contact_dialog.dart';
import '../widgets/print_view.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';
  int _selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    final db = Provider.of<AppDatabase>(context);

    return Scaffold(
      body: IndexedStack(
        index: _selectedIndex,
        children: [
          _buildContactsTab(db),
          const PrintView(),
          const Center(child: Text('Report View', style: TextStyle(color: Colors.white))),
          const Center(child: Text('Settings View', style: TextStyle(color: Colors.white))),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        backgroundColor: AppTheme.backgroundColor,
        type: BottomNavigationBarType.fixed,
        selectedItemColor: AppTheme.primaryColor,
        unselectedItemColor: AppTheme.secondaryTextColor,
        currentIndex: _selectedIndex,
        onTap: (index) {
          setState(() {
            _selectedIndex = index;
          });
        },
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.contacts), label: 'Contacts'),
          BottomNavigationBarItem(icon: Icon(Icons.print), label: 'Print'),
          BottomNavigationBarItem(icon: Icon(Icons.history), label: 'Report'),
          BottomNavigationBarItem(icon: Icon(Icons.settings), label: 'Settings'),
        ],
      ),
    );
  }

  Widget _buildContactsTab(AppDatabase db) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 20),
            const Text(
              'Contacts',
              style: TextStyle(
                fontSize: 34,
                fontWeight: FontWeight.bold,
                color: Colors.white,
              ),
            ),
            const SizedBox(height: 20),
            // Search Bar
            Container(
              decoration: BoxDecoration(
                color: const Color(0xFF1c2732),
                borderRadius: BorderRadius.circular(12),
              ),
              child: TextField(
                controller: _searchController,
                onChanged: (value) {
                  setState(() {
                    _searchQuery = value;
                  });
                },
                decoration: const InputDecoration(
                  hintText: 'Search contacts, codes...',
                  hintStyle: TextStyle(color: Color(0xFF8e97a3)),
                  prefixIcon: Icon(Icons.search, color: Color(0xFF8e97a3)),
                  border: InputBorder.none,
                  contentPadding: EdgeInsets.symmetric(vertical: 15),
                ),
              ),
            ),
            const SizedBox(height: 20),
            // Filter Buttons
            Row(
              children: [
                ElevatedButton.icon(
                  onPressed: () => _showAddDialog(context),
                  icon: const Icon(Icons.person_add_alt_1),
                  label: const Text('Add Contact'),
                ),
              ],
            ),
            const SizedBox(height: 30),
            // Contacts List
            Expanded(
              child: StreamBuilder<List<Contact>>(
                stream: _searchQuery.isEmpty ? db.watchAllContacts() : db.searchContacts(_searchQuery),
                builder: (context, snapshot) {
                  if (!snapshot.hasData) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  final contacts = snapshot.data!;
                  if (contacts.isEmpty) {
                    return const Center(
                      child: Text(
                        'No contacts found',
                        style: TextStyle(color: AppTheme.secondaryTextColor),
                      ),
                    );
                  }

                  final sortedContacts = List<Contact>.from(contacts)..sort((a, b) => a.name.compareTo(b.name));

                  return ListView.builder(
                    itemCount: sortedContacts.length,
                    itemBuilder: (context, index) {
                      return ContactListTile(contact: sortedContacts[index]);
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildIconButton(IconData icon) {
    return Container(
      decoration: BoxDecoration(
        color: const Color(0xFF1c2732),
        borderRadius: BorderRadius.circular(12),
      ),
      child: IconButton(
        icon: Icon(icon, color: const Color(0xFF8e97a3)),
        onPressed: () {},
      ),
    );
  }

  void _showAddDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => const ContactDialog(),
    );
  }
}
