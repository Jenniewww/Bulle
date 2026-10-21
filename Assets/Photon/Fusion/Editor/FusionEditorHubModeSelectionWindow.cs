namespace Fusion.Editor {
  using UnityEditor;
  using UnityEngine;

  public class FusionEditorHubModeSelectionWindow : EditorWindow
  {
    /// <summary>
    /// The Fusion Hub Unity skin.
    /// </summary>
    public GUISkin HubSkin;
    
    public Texture2D BeginnerImage;
    public Texture2D AdvancedImage;
    
    private const float ButtonWidth = 400f;
    private const float ButtonHeight = 220f;
    private const float WindowWidth = ButtonWidth * 2 + 30;
    private const float WindowHeight = ButtonHeight + 40;

    private const string FooterNoteText = "<i>Can later be toggled in the \"welcome\" section of the hub.</i>";// +
                                          // "\n\n" +
                                          // "Unsure whether you require Host / Server Mode? " +
                                          // "More information about choosing the right mode can be found " +
                                          // "<a href=\\\"https://example.com\\\">here</a>.";
                                          
    public static void ShowWindow()
    {
      var window = GetWindow<FusionEditorHubModeSelectionWindow>(true, "Choose Fusion Hub Mode", true);
      window.minSize = new Vector2(WindowWidth, WindowHeight);
      window.maxSize = new Vector2(WindowWidth, WindowHeight);
      window.ShowPopup();
    }
    
    private void OnGUI()
    {
      GUILayout.Space(10);

      EditorGUILayout.BeginHorizontal();

      GUILayout.Space(10);
      DrawButton("For Beginners", "Display beginner content <b>only</b>. " +
                                  "We recommend you use this if you are new to Multiplayer or Fusion. Shared Mode only.",
        BeginnerImage, () => {OnModeSelectButtonPressed(NetworkProjectConfig.FusionHubMode.Beginner);});
      GUILayout.Space(10);
      DrawButton("Advanced", "Display all content, including Host / Server Mode content. For existing Fusion users.",
        AdvancedImage, () => { OnModeSelectButtonPressed(NetworkProjectConfig.FusionHubMode.Advanced);});
      GUILayout.Space(10);
      
      EditorGUILayout.EndHorizontal();
      
      GUILayout.Space(10);
      
      var footerLabelStyle = new GUIStyle(HubSkin.label);
      footerLabelStyle.richText = true;
      footerLabelStyle.wordWrap = true;
      float labelHeight = footerLabelStyle.CalcHeight(new GUIContent(FooterNoteText), position.width);
      
      Rect labelBoxRect = new Rect(10, position.height - labelHeight, position.width, labelHeight);
      Color originalColor = GUI.color; // Save original color
      GUI.color = new Color(0, 0, 0, 0.1f); // Semi-transparent black
      GUI.Box(labelBoxRect, GUIContent.none); // Draw transparent background
      GUI.color = originalColor; // Restore original color
   
      GUI.Label(labelBoxRect, FooterNoteText, footerLabelStyle);
    }

    private void OnModeSelectButtonPressed(NetworkProjectConfig.FusionHubMode mode) {
      FusionEditorHubWindowSdk.SwitchHubMode(mode);
      
      Close();
    }
    
    private void DrawButton(string title, string description, Texture2D backgroundImage, System.Action onClick)
    {
      Rect buttonRect = GUILayoutUtility.GetRect(ButtonWidth, ButtonHeight);
      GUI.Box(buttonRect, GUIContent.none, EditorStyles.helpBox);

      var color = GUI.color;
      GUI.color = new Color32(200, 200, 200, 255);
      
      GUIStyle buttonStyle = new GUIStyle();
      buttonStyle.normal.background = backgroundImage;
      buttonStyle.border = new RectOffset(0, 0, 0, 0);
      buttonStyle.margin = new RectOffset(0, 0, 0, 0);
      buttonStyle.padding = new RectOffset(0, 0, 0, 0);
    
      
      if (buttonRect.Contains(Event.current.mousePosition)) {
        GUI.color = new Color32(255, 255, 255, 255);
      }
      
      if (GUI.Button(buttonRect, GUIContent.none, buttonStyle))
      {
        onClick?.Invoke();
      }

      GUI.color = color;
      
      
      var descriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
      descriptionStyle.richText = true;
      descriptionStyle.fontSize = 13;
      float descriptionWidth = ButtonWidth - 20;
      float labelHeight = 50;//47.5f;//descriptionStyle.CalcHeight(new GUIContent(description), descriptionWidth) + 5;
      GUI.Label(new Rect(buttonRect.x + 10, buttonRect.y + ButtonHeight - labelHeight, descriptionWidth, labelHeight), description, descriptionStyle);
    }
  }
}
