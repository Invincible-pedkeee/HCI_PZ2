using System.Windows;
using System.Windows.Controls;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class NetworkDisplayView : UserControl
    {
        private bool isDragging;
        private NetworkEntity draggedEntity;

        public NetworkDisplayView()
        {
            InitializeComponent();
        }

        private void EntitiesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isDragging)
            {
                return;
            }

            if (EntitiesTreeView.SelectedItem is NetworkEntity selectedEntity)
            {
                isDragging = true;
                draggedEntity = selectedEntity;

                DragDrop.DoDragDrop(this, draggedEntity, DragDropEffects.Move);
            }
        }

        private void EntitiesTreeView_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ResetDragState();
        }

        private void DisplaySlot_DragOver(object sender, DragEventArgs e)
        {
            DisplaySlot slot = GetDisplaySlotFromSender(sender);
            NetworkEntity entity = GetDraggedEntityFromEvent(e);

            if (entity != null && slot != null && !slot.IsOccupied)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void DisplaySlot_Drop(object sender, DragEventArgs e)
        {
            DisplaySlot slot = GetDisplaySlotFromSender(sender);
            NetworkEntity entity = GetDraggedEntityFromEvent(e);

            if (entity != null && slot != null)
            {
                NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

                if (viewModel != null)
                {
                    viewModel.DropEntityToSlot(entity, slot);
                }
            }

            ResetDragState();

            e.Handled = true;
        }

        private void RemoveSlotButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            DisplaySlot slot = button.DataContext as DisplaySlot;

            if (slot == null)
            {
                return;
            }

            NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

            if (viewModel != null)
            {
                viewModel.RemoveEntityFromSlot(slot);
            }
        }

        private DisplaySlot GetDisplaySlotFromSender(object sender)
        {
            Border border = sender as Border;

            if (border == null)
            {
                return null;
            }

            return border.DataContext as DisplaySlot;
        }

        private NetworkEntity GetDraggedEntityFromEvent(DragEventArgs e)
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

        private void ResetDragState()
        {
            isDragging = false;
            draggedEntity = null;
        }
    }
}