using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace EditorTreeView
{
	internal class PrefabHierachyTreeView : TreeViewWithTreeModel<PrefabHierachyElement>
	{
		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
		
		enum Columns
		{
			Name,
			Type,
			Category,
            RustID,
		}

		public enum SortOption
		{
			Name,
			Type,
            Category,
            RustID,
        }

		SortOption[] m_SortOptions = 
		{
			SortOption.Name, 
			SortOption.Type,
			SortOption.Category,
            SortOption.RustID,
        };

		public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();
	
			if (root.children == null)
				return;

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (int i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		public PrefabHierachyTreeView (TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<PrefabHierachyElement> model) : base (state, multicolumnHeader, model)
		{
			Assert.AreEqual(m_SortOptions.Length , Enum.GetValues(typeof(Columns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			extraSpaceBeforeIconAndLabel = kToggleWidth;
			multicolumnHeader.sortingChanged += OnSortingChanged;
			Reload();
		}
        public static List<PrefabHierachyElement> GetPrefabHierachyElements()
        {
            List<PrefabHierachyElement> prefabHierachyElements = new List<PrefabHierachyElement>();
            prefabHierachyElements.Add(new PrefabHierachyElement("", -1, -1));
            var prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
            for (int i = 0; i < prefabs.Length; i++)
            {
                string name = String.Format("{0}:{1}:{2}:{3}", prefabs[i].name.Replace(':', ' '), "Rust", prefabs[i].prefabData.category, prefabs[i].prefabData.id);
                prefabHierachyElements.Add(new PrefabHierachyElement(name, 0, i));
            }
            return prefabHierachyElements;
        }

		// Note we only build the visible rows, only the backend has the full tree information. 
		// The treeview only creates info for the row list.
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows (root);
			SortIfNeeded (root, rows);
			return rows;
		}

		void OnSortingChanged (MultiColumnHeader multiColumnHeader)
		{
			SortIfNeeded (rootItem, GetRows());
		}

		void SortIfNeeded (TreeViewItem root, IList<TreeViewItem> rows)
		{
			if (rows.Count <= 1)
				return;
			
			if (multiColumnHeader.sortedColumnIndex == -1)
			{
				return; // No column to sort for (just use the order the data are in)
			}
			
			// Sort the roots of the existing tree items
			SortByMultipleColumns ();
			TreeToList(root, rows);
			Repaint();
		}

		void SortByMultipleColumns ()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<TreeViewItem<PrefabHierachyElement> >();
			var orderedQuery = InitialOrder (myTypes, sortedColumns);
			for (int i=1; i<sortedColumns.Length; i++)
			{
				SortOption sortOption = m_SortOptions[sortedColumns[i]];
				bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.Name:
						orderedQuery = orderedQuery.ThenBy(l => l.data.prefabName, ascending);
						break;
					case SortOption.Type:
						orderedQuery = orderedQuery.ThenBy(l => l.data.type, ascending);
						break;
					case SortOption.Category:
						orderedQuery = orderedQuery.ThenBy(l => l.data.category, ascending);
						break;
                    case SortOption.RustID:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.rustID, ascending);
                        break;
                }
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem> ().ToList ();
		}

		IOrderedEnumerable<TreeViewItem<PrefabHierachyElement>> InitialOrder(IEnumerable<TreeViewItem<PrefabHierachyElement>> myTypes, int[] history)
		{
			SortOption sortOption = m_SortOptions[history[0]];
			bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (sortOption)
			{
				case SortOption.Name:
					return myTypes.Order(l => l.data.prefabName, ascending);
				case SortOption.Type:
					return myTypes.Order(l => l.data.type, ascending);
				case SortOption.Category:
					return myTypes.Order(l => l.data.category, ascending);
                case SortOption.RustID:
                    return myTypes.Order(l => l.data.rustID, ascending);
            }
			return myTypes.Order(l => l.data.name, ascending);
		}

		protected override void RowGUI (RowGUIArgs args)
		{
			var item = (TreeViewItem<PrefabHierachyElement>) args.item;

			for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		void CellGUI (Rect cellRect, TreeViewItem<PrefabHierachyElement> item, Columns column, ref RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column)
			{
				case Columns.Name:
                    Rect textRect = cellRect;
                    textRect.x += GetContentIndent(item);
                    textRect.xMax = cellRect.xMax - textRect.x;
                    item.data.prefabName = EditorGUI.TextField(textRect, item.data.prefabName, EditorStyles.label); 
                    break;
				case Columns.Type:
                    GUI.Label(cellRect, item.data.type);
					break;
                case Columns.Category:
                    GUI.Label(cellRect, item.data.category);
                    break;
                case Columns.RustID:
                    GUI.Label(cellRect, item.data.rustID.ToString());
                    break;
            }
		}

		// Rename
		//--------

		protected override bool CanRename(TreeViewItem item)
		{
			// Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
			Rect renameRect = GetRenameRect (treeViewRect, 0, item);
			return renameRect.width > 30;
		}

		protected override void RenameEnded(RenameEndedArgs args)
		{
			// Set the backend name and reload the tree to reflect the new model
			if (args.acceptedRename)
			{
				var element = treeModel.Find(args.itemID);
				element.name = args.newName;
				Reload();
			}
		}

		protected override Rect GetRenameRect (Rect rowRect, int row, TreeViewItem item)
		{
			Rect cellRect = GetCellRectForTreeFoldouts (rowRect);
			CenterRectUsingSingleLineHeight(ref cellRect);
			return base.GetRenameRect (cellRect, row, item);
		}

		// Misc
		//--------

		protected override bool CanMultiSelect (TreeViewItem item)
		{
			return true;
		}
	}

	static class ExtensionMethods
	{
		public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.OrderBy(selector);
			}
			else
			{
				return source.OrderByDescending(selector);
			}
		}

		public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.ThenBy(selector);
			}
			else
			{
				return source.ThenByDescending(selector);
			}
		}
	}
}
