using Chapter6.Interface;

namespace Chapter6.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private SpellCheckViewModel _spellCheckVm;
        public SpellCheckViewModel SpellCheckVm
        {
            get { return _spellCheckVm; }
            set
            {
                _spellCheckVm = value;
                RaisePropertyChangedEvent("SpellCheckVm");
            }
        }

        private TextAnalysisViewModel _textAnalysisVm;
        public TextAnalysisViewModel TextAnalysisVm
        {
            get { return _textAnalysisVm; }
            set
            {
                _textAnalysisVm = value;
                RaisePropertyChangedEvent("TextAnalysisVm");
            }
        }
        
        /// <summary>
        /// MainViewModel constructor creates all other view model objects
        /// </summary>
        public MainViewModel()
        {
            SpellCheckVm = new SpellCheckViewModel();
            TextAnalysisVm = new TextAnalysisViewModel();
        }
    }
}