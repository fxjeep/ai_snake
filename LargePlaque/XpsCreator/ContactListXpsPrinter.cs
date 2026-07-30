using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using PlaqueData.Models;

namespace XpsCreator
{
    public static class ContactListXpsPrinter
    {
        public static string GenerateContactListXps(
            string outputXpsPath,
            Contact contact,
            List<Live> liveRecords,
            List<Dead> deadRecords,
            List<Ancestor> ancestorRecords,
            List<PlaqueData.Models.Property> propertyRecords)
        {
            // Ensure directory exists
            string? dir = Path.GetDirectoryName(outputXpsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Create title: [ContactCode - ContactName - yyyy-MM-dd]
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string titleText = $"[{contact.Code} - {contact.Name} - {dateStr}]";

            var pageManager = new PageManager(titleText);

            // 1. Live Table (4 columns)
            if (liveRecords != null && liveRecords.Count > 0)
            {
                var liveRows = new List<string[]>();
                int cols = 4;
                for (int i = 0; i < liveRecords.Count; i += cols)
                {
                    var row = new string[cols];
                    for (int c = 0; c < cols; c++)
                    {
                        if (i + c < liveRecords.Count)
                        {
                            row[c] = liveRecords[i + c].Name ?? string.Empty;
                        }
                        else
                        {
                            row[c] = string.Empty;
                        }
                    }
                    liveRows.Add(row);
                }

                double[] liveColWidths = { 1.0, 1.0, 1.0, 1.0 };
                AddTableToPage(pageManager, "Live Records", liveColWidths, null, liveRows);
            }

            // 2. Dead Table (6 columns: 2 records * 3 columns)
            if (deadRecords != null && deadRecords.Count > 0)
            {
                var deadRows = new List<string[]>();
                for (int i = 0; i < deadRecords.Count; i += 2)
                {
                    var row = new string[6];
                    // Record 1
                    if (i < deadRecords.Count)
                    {
                        row[0] = deadRecords[i].DeadName ?? string.Empty;
                        row[1] = deadRecords[i].LiveName ?? string.Empty;
                        row[2] = deadRecords[i].Relation ?? string.Empty;
                    }
                    else
                    {
                        row[0] = string.Empty;
                        row[1] = string.Empty;
                        row[2] = string.Empty;
                    }

                    // Record 2
                    if (i + 1 < deadRecords.Count)
                    {
                        row[3] = deadRecords[i + 1].DeadName ?? string.Empty;
                        row[4] = deadRecords[i + 1].LiveName ?? string.Empty;
                        row[5] = deadRecords[i + 1].Relation ?? string.Empty;
                    }
                    else
                    {
                        row[3] = string.Empty;
                        row[4] = string.Empty;
                        row[5] = string.Empty;
                    }

                    deadRows.Add(row);
                }

                double[] deadColWidths = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
                string[] deadHeaders = { "Dead Name", "Live Name", "Relation", "Dead Name", "Live Name", "Relation" };
                AddTableToPage(pageManager, "Dead Records", deadColWidths, deadHeaders, deadRows);
            }

            // 3. Ancestor Table (6 columns: 3 records * 2 columns)
            if (ancestorRecords != null && ancestorRecords.Count > 0)
            {
                var ancestorRows = new List<string[]>();
                for (int i = 0; i < ancestorRecords.Count; i += 3)
                {
                    var row = new string[6];
                    // Record 1
                    if (i < ancestorRecords.Count)
                    {
                        row[0] = ancestorRecords[i].Surname ?? string.Empty;
                        row[1] = ancestorRecords[i].Name ?? string.Empty;
                    }
                    else
                    {
                        row[0] = string.Empty;
                        row[1] = string.Empty;
                    }

                    // Record 2
                    if (i + 1 < ancestorRecords.Count)
                    {
                        row[2] = ancestorRecords[i + 1].Surname ?? string.Empty;
                        row[3] = ancestorRecords[i + 1].Name ?? string.Empty;
                    }
                    else
                    {
                        row[2] = string.Empty;
                        row[3] = string.Empty;
                    }

                    // Record 3
                    if (i + 2 < ancestorRecords.Count)
                    {
                        row[4] = ancestorRecords[i + 2].Surname ?? string.Empty;
                        row[5] = ancestorRecords[i + 2].Name ?? string.Empty;
                    }
                    else
                    {
                        row[4] = string.Empty;
                        row[5] = string.Empty;
                    }

                    ancestorRows.Add(row);
                }

                double[] ancestorColWidths = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
                string[] ancestorHeaders = { "Surname", "Live Name", "Surname", "Live Name", "Surname", "Live Name" };
                AddTableToPage(pageManager, "Ancestor Records", ancestorColWidths, ancestorHeaders, ancestorRows);
            }

            // 4. Property Table (2 columns: 1 record * 2 columns)
            if (propertyRecords != null && propertyRecords.Count > 0)
            {
                var propertyRows = new List<string[]>();
                foreach (var prop in propertyRecords)
                {
                    propertyRows.Add(new string[] { prop.LiveName ?? string.Empty, prop.Address ?? string.Empty });
                }

                double[] propertyColWidths = { 1.5, 3.5 };
                string[] propertyHeaders = { "Live Name", "Address" };
                AddTableToPage(pageManager, "Property Records", propertyColWidths, propertyHeaders, propertyRows);
            }

            // If absolutely no records were printed, show a placeholder
            if (pageManager.IsEmpty)
            {
                var noRecordsBlock = new TextBlock
                {
                    Text = "No records found.",
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 14.0 * 96.0 / 72.0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                pageManager.AddElement(noRecordsBlock);
            }

            pageManager.FinalizePageNumbers();

            // Save FixedDocument to XPS file
            using (var package = Package.Open(outputXpsPath, FileMode.Create))
            {
                var xpsDoc = new XpsDocument(package);
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                foreach (var page in pageManager.Pages)
                {
                    var content = new PageContent();
                    ((System.Windows.Markup.IAddChild)content).AddChild(page);
                    fixedDoc.Pages.Add(content);
                }

                writer.Write(fixedDoc);
                xpsDoc.Close();
            }

            return outputXpsPath;
        }

        private static UIElement CreateSectionHeader(string title)
        {
            return new TextBlock
            {
                Text = title,
                FontFamily = new FontFamily("SongTi"),
                FontSize = 12.0 * 96.0 / 72.0,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 3),
                Foreground = Brushes.DarkSlateGray
            };
        }

        private static Grid CreateTableRow(double[] colWidths, string[] cellTexts, bool isHeader = false)
        {
            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            for (int col = 0; col < colWidths.Length; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(colWidths[col], GridUnitType.Star)
                });
            }

            for (int col = 0; col < cellTexts.Length; col++)
            {
                var border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(2),
                    Background = isHeader ? new SolidColorBrush(Color.FromRgb(240, 240, 240)) : Brushes.Transparent
                };

                var textBlock = new TextBlock
                {
                    Text = cellTexts[col],
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 10.0 * 96.0 / 72.0, // 10pt
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                border.Child = textBlock;
                Grid.SetColumn(border, col);
                grid.Children.Add(border);
            }

            return grid;
        }

        private static void AddTableToPage(PageManager pageManager, string sectionTitle, double[] colWidths, string[]? headerTexts, List<string[]> rowData)
        {
            // Add section header
            var sectionHeader = CreateSectionHeader(sectionTitle);
            pageManager.AddElement(sectionHeader);
            pageManager.MarkNotEmpty();

            // Add table header row if provided
            if (headerTexts != null)
            {
                var headerRow = CreateTableRow(colWidths, headerTexts, isHeader: true);
                pageManager.AddElement(headerRow);
            }

            // Add table rows
            foreach (var rowFields in rowData)
            {
                var row = CreateTableRow(colWidths, rowFields, isHeader: false);

                int prevPageCount = pageManager.Pages.Count;
                pageManager.AddElement(row);
                int newPageCount = pageManager.Pages.Count;

                if (newPageCount > prevPageCount)
                {
                    // A new page was started when adding this row.
                    // Remove the row from the new page's container first
                    pageManager.RemoveLastElement(row);

                    // Add the table header if provided
                    if (headerTexts != null)
                    {
                        var newHeaderRow = CreateTableRow(colWidths, headerTexts, isHeader: true);
                        pageManager.AddElement(newHeaderRow);
                    }

                    // Add the row back
                    pageManager.AddElement(row);
                }
            }
        }

        private class PageManager
        {
            private readonly string _titleText;
            private readonly List<FixedPage> _pages = new();
            private FixedPage _currentPage = null!;
            private StackPanel _currentPanel = null!;
            private double _maxContentHeight;
            private double _currentHeight;
            private int _pageCount = 0;
            private bool _isEmpty = true;

            public PageManager(string titleText)
            {
                _titleText = titleText;
                StartNewPage();
            }

            public List<FixedPage> Pages => _pages;
            public StackPanel CurrentPanel => _currentPanel;
            public bool IsEmpty => _isEmpty;

            public void MarkNotEmpty()
            {
                _isEmpty = false;
            }

            private void StartNewPage()
            {
                _pageCount++;
                var (page, panel) = CreatePage(_titleText, _pageCount);
                _currentPage = page;
                _currentPanel = panel;

                var mainGrid = (Grid)page.Children[0];
                var titleBlock = (TextBlock)mainGrid.Children[0];
                var pageNumberBlock = (TextBlock)mainGrid.Children[2];

                titleBlock.Measure(new Size(680.31, double.PositiveInfinity));
                double titleHeight = titleBlock.DesiredSize.Height + titleBlock.Margin.Top + titleBlock.Margin.Bottom;

                pageNumberBlock.Measure(new Size(680.31, double.PositiveInfinity));
                double footerHeight = pageNumberBlock.DesiredSize.Height + pageNumberBlock.Margin.Top + pageNumberBlock.Margin.Bottom;

                _maxContentHeight = 1084.73 - titleHeight - footerHeight;
                _currentHeight = 0;
                _pages.Add(page);
            }

            private static (FixedPage Page, StackPanel ContentPanel) CreatePage(string titleText, int pageNumber)
            {
                var fixedPage = new FixedPage
                {
                    Width = 793.70,
                    Height = 1122.52
                };

                var mainGrid = new Grid
                {
                    Width = 680.31,
                    Height = 1084.73,
                    Margin = new Thickness(56.69, 37.79, 56.69, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var titleBlock = new TextBlock
                {
                    Text = titleText,
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 16.0 * 96.0 / 72.0, // 16pt font
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                Grid.SetRow(titleBlock, 0);
                mainGrid.Children.Add(titleBlock);

                var contentPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(contentPanel, 1);
                mainGrid.Children.Add(contentPanel);

                var pageNumberBlock = new TextBlock
                {
                    Text = pageNumber.ToString(),
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 12.0 * 96.0 / 72.0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 52.80, 0, 0)
                };
                Grid.SetRow(pageNumberBlock, 2);
                mainGrid.Children.Add(pageNumberBlock);

                fixedPage.Children.Add(mainGrid);

                fixedPage.Measure(new Size(793.70, 1122.52));
                fixedPage.Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                fixedPage.UpdateLayout();

                return (fixedPage, contentPanel);
            }

            public void AddElement(UIElement element)
            {
                element.Measure(new Size(680.31, double.PositiveInfinity));
                double elementHeight = element.DesiredSize.Height;

                if (_currentHeight + elementHeight > _maxContentHeight && _currentHeight > 0)
                {
                    StartNewPage();
                    element.Measure(new Size(680.31, double.PositiveInfinity));
                    elementHeight = element.DesiredSize.Height;
                }

                _currentPanel.Children.Add(element);
                _currentHeight += elementHeight;

                _currentPage.Measure(new Size(793.70, 1122.52));
                _currentPage.Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                _currentPage.UpdateLayout();
            }

            public void RemoveLastElement(UIElement element)
            {
                if (_currentPanel.Children.Contains(element))
                {
                    _currentPanel.Children.Remove(element);
                    element.Measure(new Size(680.31, double.PositiveInfinity));
                    _currentHeight -= element.DesiredSize.Height;
                    if (_currentHeight < 0) _currentHeight = 0;
                }
            }

            public void FinalizePageNumbers()
            {
                for (int i = 0; i < _pages.Count; i++)
                {
                    var mainGrid = (Grid)_pages[i].Children[0];
                    var pageNumberBlock = (TextBlock)mainGrid.Children[2];
                    pageNumberBlock.Text = $"{i + 1}";
                    
                    _pages[i].Measure(new Size(793.70, 1122.52));
                    _pages[i].Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                    _pages[i].UpdateLayout();
                }
            }
        }

        public static string GeneratePrintListXps(
            string outputXpsPath,
            List<ContactPrintSummary> summaries)
        {
            string? dir = Path.GetDirectoryName(outputXpsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string titleText = $"{dateStr} Print List";

            var pageManager = new SummaryPageManager(titleText);

            double[] colWidths = Enumerable.Repeat(1.0, 8).ToArray();

            var orderedSummaries = summaries.OrderBy(s => s.Code).ToList();
            for (int i = 0; i < orderedSummaries.Count; i += 4)
            {
                var rowTexts = new List<string>();
                for (int j = 0; j < 4; j++)
                {
                    if (i + j < orderedSummaries.Count)
                    {
                        rowTexts.Add(orderedSummaries[i + j].Name);
                        rowTexts.Add(orderedSummaries[i + j].Code);
                    }
                    else
                    {
                        rowTexts.Add("");
                        rowTexts.Add("");
                    }
                }
                var row = CreateSummaryTableRow(colWidths, rowTexts.ToArray(), isHeader: false);
                if (i == 0)
                    pageManager.AddInitialElement(row);
                else
                    pageManager.AddElement(row);
            }

            pageManager.FinalizePageNumbers();

            using (var package = Package.Open(outputXpsPath, FileMode.Create))
            {
                var xpsDoc = new XpsDocument(package);
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                foreach (var page in pageManager.Pages)
                {
                    var content = new PageContent();
                    ((System.Windows.Markup.IAddChild)content).AddChild(page);
                    fixedDoc.Pages.Add(content);
                }

                writer.Write(fixedDoc);
                xpsDoc.Close();
            }

            return outputXpsPath;
        }

        public static string GenerateContactsSummaryXps(
            string outputXpsPath,
            List<Contact> contacts,
            int selectedYear)
        {
            // Ensure directory exists
            string? dir = Path.GetDirectoryName(outputXpsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string titleText = $"Contact List Since {selectedYear}";
            var pageManager = new SummaryPageManager(titleText);

            double[] colWidths = { 2.0, 1.2, 1.8 };

            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                string formattedDate = contact.LastPrint;
                if (DateTime.TryParse(contact.LastPrint, out DateTime dt))
                {
                    formattedDate = dt.ToString("yyyy-MM-dd");
                }
                string[] rowTexts = { contact.Name, contact.Code, formattedDate };
                var row = CreateSummaryTableRow(colWidths, rowTexts, isHeader: false);

                if (i == 0)
                    pageManager.AddInitialElement(row);
                else
                    pageManager.AddElement(row);
            }

            pageManager.FinalizePageNumbers();

            // Save FixedDocument to XPS file
            using (var package = Package.Open(outputXpsPath, FileMode.Create))
            {
                var xpsDoc = new XpsDocument(package);
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                foreach (var page in pageManager.Pages)
                {
                    var content = new PageContent();
                    ((System.Windows.Markup.IAddChild)content).AddChild(page);
                    fixedDoc.Pages.Add(content);
                }

                writer.Write(fixedDoc);
                xpsDoc.Close();
            }

            return outputXpsPath;
        }

        private static Grid CreateSummaryTableRow(double[] colWidths, string[] cellTexts, bool isHeader = false)
        {
            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            for (int col = 0; col < colWidths.Length; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(colWidths[col], GridUnitType.Star)
                });
            }

            for (int col = 0; col < cellTexts.Length; col++)
            {
                var border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1), // Full border
                    Padding = new Thickness(0),
                    Background = isHeader ? new SolidColorBrush(Color.FromRgb(240, 240, 240)) : Brushes.Transparent
                };

                var textBlock = new TextBlock
                {
                    Text = cellTexts[col] ?? string.Empty,
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 26.0, // Font size is 26px
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                border.Child = textBlock;
                Grid.SetColumn(border, col);
                grid.Children.Add(border);
            }

            return grid;
        }

        private class SummaryPageManager
        {
            private readonly string _titleText;
            private readonly List<FixedPage> _pages = new();
            private FixedPage _currentPage = null!;
            private StackPanel _currentPanel = null!;
            private double _maxContentHeight;
            private double _currentHeight;
            private int _pageCount = 0;

            public SummaryPageManager(string titleText)
            {
                _titleText = titleText;
                StartNewPage();
            }

            public List<FixedPage> Pages => _pages;
            public StackPanel CurrentPanel => _currentPanel;

            private void StartNewPage()
            {
                _pageCount++;
                var (page, panel) = CreatePage(_titleText, _pageCount);
                _currentPage = page;
                _currentPanel = panel;

                var mainGrid = (Grid)page.Children[0];
                var titleBlock = (TextBlock)mainGrid.Children[0];
                var pageNumberBlock = (TextBlock)mainGrid.Children[2];

                titleBlock.Measure(new Size(680.31, double.PositiveInfinity));
                double titleHeight = titleBlock.DesiredSize.Height + titleBlock.Margin.Top + titleBlock.Margin.Bottom;

                pageNumberBlock.Measure(new Size(680.31, double.PositiveInfinity));
                double footerHeight = pageNumberBlock.DesiredSize.Height + pageNumberBlock.Margin.Top + pageNumberBlock.Margin.Bottom;

                _maxContentHeight = 1079.73 - titleHeight - footerHeight;
                _currentHeight = 0;
                _pages.Add(page);
            }

            private static (FixedPage Page, StackPanel ContentPanel) CreatePage(string titleText, int pageNumber)
            {
                var fixedPage = new FixedPage
                {
                    Width = 793.70,
                    Height = 1122.52
                };

                var mainGrid = new Grid
                {
                    Width = 680.31,
                    Height = 1079.73,
                    Margin = new Thickness(56.69, 37.79, 56.69, 5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var titleBlock = new TextBlock
                {
                    Text = titleText,
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 28.0,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, -18.9, 0, 15)
                };
                Grid.SetRow(titleBlock, 0);
                mainGrid.Children.Add(titleBlock);

                var contentPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(contentPanel, 1);
                mainGrid.Children.Add(contentPanel);

                var pageNumberBlock = new TextBlock
                {
                    Text = pageNumber.ToString(),
                    FontFamily = new FontFamily("SongTi"),
                    FontSize = 20.0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                Grid.SetRow(pageNumberBlock, 2);
                mainGrid.Children.Add(pageNumberBlock);

                fixedPage.Children.Add(mainGrid);

                fixedPage.Measure(new Size(793.70, 1122.52));
                fixedPage.Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                fixedPage.UpdateLayout();

                return (fixedPage, contentPanel);
            }

            public void AddInitialElement(UIElement element)
            {
                element.Measure(new Size(680.31, double.PositiveInfinity));
                _currentPanel.Children.Add(element);
                _currentHeight += element.DesiredSize.Height;

                _currentPage.Measure(new Size(793.70, 1122.52));
                _currentPage.Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                _currentPage.UpdateLayout();
            }

            public void AddElement(UIElement element)
            {
                element.Measure(new Size(680.31, double.PositiveInfinity));
                double elementHeight = element.DesiredSize.Height;

                if (_currentHeight + elementHeight > _maxContentHeight && _currentHeight > 0)
                    StartNewPage();

                _currentPanel.Children.Add(element);
                _currentHeight += elementHeight;

                _currentPage.Measure(new Size(793.70, 1122.52));
                _currentPage.Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                _currentPage.UpdateLayout();
            }

            public void FinalizePageNumbers()
            {
                for (int i = 0; i < _pages.Count; i++)
                {
                    var mainGrid = (Grid)_pages[i].Children[0];
                    var pageNumberBlock = (TextBlock)mainGrid.Children[2];
                    pageNumberBlock.Text = $"{i + 1} of {_pages.Count} page";
                    
                    _pages[i].Measure(new Size(793.70, 1122.52));
                    _pages[i].Arrange(new Rect(new Point(), new Size(793.70, 1122.52)));
                    _pages[i].UpdateLayout();
                }
            }
        }
    }
}
