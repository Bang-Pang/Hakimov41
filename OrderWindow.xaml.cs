using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hakimov41   
{
    public partial class OrderWindow : Window
    {
        private class OrderItemView
        {
            public Product Product { get; set; }
            public string ProductName => Product.ProductName;
            public string ProductDescription => Product.ProductDescription;
            public string ProductManufacturer => Product.ProductManufacturer;
            public decimal ProductCost => Product.ProductCost;
            public byte ProductDiscountAmount => Product.ProductDiscountAmount;
            public string ProductPhotoPath
                => string.IsNullOrEmpty(Product.ProductPhoto)
                   ? null
                   : System.IO.Path.Combine("..", "res", Product.ProductPhoto);
            public int Quantity { get; set; }
        }


        private readonly User _currentUser;
        private readonly List<Product> _selectedProducts;
        private readonly List<OrderItemView> _orderItems = new List<OrderItemView>();
        private readonly Order _currentOrder = new Order();

        public OrderWindow(List<Product> selectedProducts, User user)
        {
            InitializeComponent();

            _selectedProducts = selectedProducts;
            _currentUser = user;

            OrderDateText.Text = DateTime.Now.ToShortDateString();

            if (_currentUser != null)
                ClientNameText.Text = $"{_currentUser.UserSurname} {_currentUser.UserName} {_currentUser.UserPatronymic}";
            else
                ClientNameText.Text = "";

            var context = Hakimov41Entities1.GetContext();

            var pickupPoints = context.PickUpPoint
                .ToList()
                .Select(p => new
                {
                    p.PickUpPointID,
                    FullAddress = $"{p.PickUpPointPostIndex} г. {p.PickUpPointCity} ул. {p.PickUpPointStreet} {p.PickUpPointHouse}"
                })
                .ToList();

            PickupPointComboBox.ItemsSource = pickupPoints;
            if (pickupPoints.Any())
                PickupPointComboBox.SelectedIndex = 0;

            int maxId = 0;
            if (context.Order.Any())
                maxId = context.Order.Max(o => o.OrderID);

            _currentOrder.OrderID = maxId + 1;
            OrderNumberText.Text = _currentOrder.OrderID.ToString();

            foreach (var product in _selectedProducts)
            {
                var existing = _orderItems.FirstOrDefault(i => i.Product.ProductArticleNumber == product.ProductArticleNumber);
                if (existing != null)
                    existing.Quantity++;
                else
                    _orderItems.Add(new OrderItemView { Product = product, Quantity = 1 });
            }

            OrderItemsListView.ItemsSource = _orderItems;

            
            OrderItemsListView.Items.Refresh();
            RecalculateTotals();
            UpdateDeliveryDate();
        }
        private void UpdateDeliveryDate()
        {
            bool allMoreThanThree = true;

            foreach (var item in _orderItems)
            {
                if (item.Product.ProductQuantityInStock <= 3)
                {
                    allMoreThanThree = false;
                    break;
                }
            }

            int days = allMoreThanThree ? 3 : 6;
            var deliveryDate = DateTime.Now.AddDays(days);

            DeliveryDatePicker.SelectedDate = deliveryDate;

            _currentOrder.OrderDeliveryDate = deliveryDate;
        }


        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn?.DataContext as OrderItemView;
            if (item == null) return;

            item.Quantity++;
            OrderItemsListView.Items.Refresh();
            OrderItemsListView.Items.Refresh();
            RecalculateTotals();
            UpdateDeliveryDate();

        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn?.DataContext as OrderItemView;
            if (item == null) return;

            item.Quantity--;

            if (item.Quantity <= 0)
                _orderItems.Remove(item);

            OrderItemsListView.Items.Refresh();

            if (_orderItems.Count == 0)
            {
                Close();
                return;
            }
            OrderItemsListView.Items.Refresh();
            RecalculateTotals();
            UpdateDeliveryDate();

        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn?.DataContext as OrderItemView;
            if (item == null) return;

            _orderItems.Remove(item);
            OrderItemsListView.Items.Refresh();

            if (_orderItems.Count == 0)
            {
                Close();
                return;
            }

            
            OrderItemsListView.Items.Refresh();
            RecalculateTotals();
            UpdateDeliveryDate();

        }

        private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_orderItems.Count == 0)
            {
                MessageBox.Show("Заказ пуст.");
                return;
            }

            if (PickupPointComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи.");
                return;
            }

            var context = Hakimov41Entities1.GetContext();

            _currentOrder.OrderDate = DateTime.Now;
            _currentOrder.OrderPickupPoint = (int)PickupPointComboBox.SelectedValue;
            _currentOrder.OrderStatus = "новый";

            if (_currentUser != null)
                _currentOrder.OrderClient = _currentUser.UserID;
            else
                _currentOrder.OrderClient = null;

            // код заказа (любой трёхзначный)
            var rnd = new Random();
            _currentOrder.OrderCode = rnd.Next(100, 999);

            context.Order.Add(_currentOrder);

            // создаём записи OrderProduct
            foreach (var item in _orderItems)
            {
                var op = new OrderProduct
                {
                    OrderID = _currentOrder.OrderID,
                    ProductArticleNumber = item.Product.ProductArticleNumber,
                    OrderProductCount = item.Quantity
                };
                context.OrderProduct.Add(op);
            }

            try
            {
                context.SaveChanges();
                MessageBox.Show("Заказ успешно сохранён.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении заказа: " + ex.Message);
            }
        }

        private void DeliveryDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeliveryDatePicker.SelectedDate.HasValue)
                _currentOrder.OrderDeliveryDate = DeliveryDatePicker.SelectedDate.Value;
        }
        private void RecalculateTotals()
        {
            decimal total = 0;        
            decimal discountSum = 0;  

            foreach (var item in _orderItems)
            {
                var product = item.Product;
                if (product == null) continue;

                decimal price = product.ProductCost;                  
                int count = item.Quantity;                            
                decimal discountPercent = product.ProductDiscountAmount; 

                decimal line = price * count;                         
                decimal lineDiscount = line * discountPercent / 100m; 

                total += line;
                discountSum += lineDiscount;
            }

            TotalSumText.Text = $"Сумма заказа: {total - discountSum:0.00} руб.";
            DiscountSumText.Text = $"Сумма скидки: {discountSum:0.00} руб.";

        }


    }
}
