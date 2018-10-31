using Chapter2.ViewModel;
using Microsoft.ProjectOxford.Vision;
using System.Windows.Controls;

namespace Chapter2.View
{
    /// <summary>
    /// Interaction logic for ImageAnalysisView.xaml
    /// </summary>
    public partial class ImageAnalysisView : UserControl
    {
        public ImageAnalysisView()
        {
            InitializeComponent();
        }

        private void VisualFeatures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            vm.ImageAnalysisVm.SelectedFeatures.Clear();

            foreach(VisualFeature feature in VisualFeatures.SelectedItems)
            {
                vm.ImageAnalysisVm.SelectedFeatures.Add(feature);
            }
        }
    }
}
