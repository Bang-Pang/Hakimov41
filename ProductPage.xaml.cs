using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hakimov41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage(User user)
        {
            InitializeComponent();
            if (user != null)
            {
                FioTB.Text = $"{user.UserSurname} {user.UserName} {user.UserPatronymic}";
                switch (user.UserRole)
                {
                    case 1: RoleTB.Text = "Клиент"; break;
                    case 2: RoleTB.Text = "Менеджер"; break;
                    case 3: RoleTB.Text = "Администратор"; break;
                }
            }
            else
            {
                FioTB.Text = "Гость";
                RoleTB.Text = "Гость";
            }
            var currentProduct = Hakimov41Entities1.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProduct;
            //ComboType.SelectedIndex = 0;
            //UpdateProduct();
        }

        private void UpdateProduct()
        {
            var allProducts = Hakimov41Entities1.GetContext().Product.ToList();

            var currentProducts = allProducts;

            if (ComboType.SelectedIndex == 0)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) <= 100)).ToList();
            }
            if (ComboType.SelectedIndex == 1)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 && Convert.ToDouble(p.ProductDiscountAmount) < 9.99)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 10 && Convert.ToDouble(p.ProductDiscountAmount) <= 14.99)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 15 && Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
            }

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            ProductListView.ItemsSource = currentProducts.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            }
            if (RButtonUp.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentProducts.OrderBy(p => p.ProductCost).ToList();
            }

            CountTextBlock.Text = $"{currentProducts.Count} из {allProducts.Count}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }
    }
}
