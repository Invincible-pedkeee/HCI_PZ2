using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Helpers.NetworkDisplay
{
    public class NetworkDisplayDragDropBehavior
    {
        private static bool isDragging;
        private static NetworkEntity draggedEntity;
        private static DisplaySlot sourceSlot;

        public static readonly DependencyProperty EnableTreeViewEntityDragProperty =
            DependencyProperty.RegisterAttached(
                "EnableTreeViewEntityDrag",
                typeof(bool),
                typeof(NetworkDisplayDragDropBehavior),
                new PropertyMetadata(false, OnEnableTreeViewEntityDragChanged));

        public static bool GetEnableTreeViewEntityDrag(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableTreeViewEntityDragProperty);
        }

        public static void SetEnableTreeViewEntityDrag(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableTreeViewEntityDragProperty, value);
        }

        private static void OnEnableTreeViewEntityDragChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = dependencyObject as TreeView;

            if (treeView == null)
            {
                return;
            }

            bool isEnabled = (bool)e.NewValue;

            if (isEnabled)
            {
                treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
                treeView.MouseLeftButtonUp += TreeView_MouseLeftButtonUp;
            }
            else
            {
                treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                treeView.MouseLeftButtonUp -= TreeView_MouseLeftButtonUp;
            }
        }

        private static void TreeView_SelectedItemChanged(
            object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            if (isDragging)
            {
                return;
            }

            NetworkEntity selectedEntity = e.NewValue as NetworkEntity;

            if (selectedEntity == null)
            {
                return;
            }

            TreeView treeView = sender as TreeView;

            if (treeView == null)
            {
                return;
            }

            isDragging = true;
            draggedEntity = selectedEntity;
            sourceSlot = null;

            DragDrop.DoDragDrop(treeView, draggedEntity, DragDropEffects.Move);

            ResetDragState();
        }

        private static void TreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDragState();
        }

        public static readonly DependencyProperty EnableSlotDragDropProperty =
            DependencyProperty.RegisterAttached(
                "EnableSlotDragDrop",
                typeof(bool),
                typeof(NetworkDisplayDragDropBehavior),
                new PropertyMetadata(false, OnEnableSlotDragDropChanged));

        public static bool GetEnableSlotDragDrop(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableSlotDragDropProperty);
        }

        public static void SetEnableSlotDragDrop(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableSlotDragDropProperty, value);
        }

        private static void OnEnableSlotDragDropChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = dependencyObject as FrameworkElement;

            if (element == null)
            {
                return;
            }

            bool isEnabled = (bool)e.NewValue;

            if (isEnabled)
            {
                element.MouseLeftButtonDown += DisplaySlot_MouseLeftButtonDown;
                element.DragOver += DisplaySlot_DragOver;
                element.Drop += DisplaySlot_Drop;
            }
            else
            {
                element.MouseLeftButtonDown -= DisplaySlot_MouseLeftButtonDown;
                element.DragOver -= DisplaySlot_DragOver;
                element.Drop -= DisplaySlot_Drop;
            }
        }

        private static void DisplaySlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                return;
            }

            if (IsOriginalSourceInsideButton(e.OriginalSource as DependencyObject))
            {
                return;
            }

            DisplaySlot slot = GetDisplaySlotFromSender(sender);

            if (slot == null || !slot.IsOccupied)
            {
                return;
            }

            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
            {
                return;
            }

            isDragging = true;
            sourceSlot = slot;
            draggedEntity = slot.OccupiedEntity;

            DragDrop.DoDragDrop(element, draggedEntity, DragDropEffects.Move);

            ResetDragState();
        }

        private static void DisplaySlot_DragOver(object sender, DragEventArgs e)
        {
            DisplaySlot targetSlot = GetDisplaySlotFromSender(sender);
            NetworkEntity entity = GetDraggedEntityFromEvent(e);

            if (entity != null && targetSlot != null && !targetSlot.IsOccupied)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private static void DisplaySlot_Drop(object sender, DragEventArgs e)
        {
            DisplaySlot targetSlot = GetDisplaySlotFromSender(sender);
            NetworkEntity entity = GetDraggedEntityFromEvent(e);
            NetworkDisplayViewModel viewModel = FindViewModel(sender as DependencyObject);

            if (entity != null && targetSlot != null && viewModel != null)
            {
                if (sourceSlot != null)
                {
                    viewModel.MoveEntityBetweenSlots(sourceSlot, targetSlot);
                }
                else
                {
                    viewModel.DropEntityToSlot(entity, targetSlot);
                }
            }

            ResetDragState();
            e.Handled = true;
        }

        private static DisplaySlot GetDisplaySlotFromSender(object sender)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element == null)
            {
                return null;
            }

            return element.DataContext as DisplaySlot;
        }

        private static NetworkEntity GetDraggedEntityFromEvent(DragEventArgs e)
        {
            if (draggedEntity != null)
            {
                return draggedEntity;
            }

            if (e.Data.GetDataPresent(typeof(NetworkEntity)))
            {
                return e.Data.GetData(typeof(NetworkEntity)) as NetworkEntity;
            }

            return null;
        }

        private static NetworkDisplayViewModel FindViewModel(DependencyObject source)
        {
            DependencyObject current = source;

            while (current != null)
            {
                FrameworkElement element = current as FrameworkElement;

                if (element != null && element.DataContext is NetworkDisplayViewModel)
                {
                    return element.DataContext as NetworkDisplayViewModel;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static bool IsOriginalSourceInsideButton(DependencyObject source)
        {
            while (source != null)
            {
                if (source is Button)
                {
                    return true;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private static void ResetDragState()
        {
            isDragging = false;
            draggedEntity = null;
            sourceSlot = null;
        }
    }
}