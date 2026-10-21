namespace Fusion.Statistics {
#if FUSION_ENABLE_UGUI
  using System;
  using System.Linq;
  using UnityEngine;
  using UnityEngine.UI;

  public class MultipleOptionsPanel : MonoBehaviour {
    public const int MAX_BUTTONS_COUNT = 10;

    [SerializeField] private Text _label;
    [SerializeField] private Transform _content;
    [SerializeField] private InputField _searchInput;

    public void Setup<T>(string label, T[] options, Func<T, string> defineButtonText, Action<T> buttonAction) {
      var buttonPrototype = _content.GetChild(0).GetComponent<Button>();
      buttonPrototype.gameObject.SetActive(false);
      _label.text = label;

      void UpdateDisplay() {
        for (int i = 1; i < _content.childCount; i++) {
          Destroy(_content.GetChild(i).gameObject);
        }

        string searchText = _searchInput.text.ToLower();

        var filteredOptions = options.Where(option => string.IsNullOrEmpty(searchText) || defineButtonText(option).ToLower().Contains(searchText)).ToList();

        // Instantiate buttons for the filtered list.
        for (int i = 0; i < MAX_BUTTONS_COUNT && i < filteredOptions.Count; i++) {
          var button = Instantiate(buttonPrototype, _content);
          var option = filteredOptions[i];
          button.GetComponentInChildren<Text>().text = defineButtonText.Invoke(option);
          button.onClick.RemoveAllListeners();
          button.onClick.AddListener(delegate { buttonAction.Invoke(option); });
          button.onClick.AddListener(delegate { Destroy(gameObject); });
          button.gameObject.SetActive(true);
        }
      }

      _searchInput.onValueChanged.RemoveAllListeners();
      _searchInput.onValueChanged.AddListener(delegate { UpdateDisplay(); });
      UpdateDisplay();
    }

    public void Close() {
      Destroy(gameObject);
    }
  }
#endif
}