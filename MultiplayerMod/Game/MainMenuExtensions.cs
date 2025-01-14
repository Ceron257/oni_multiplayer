﻿namespace MultiplayerMod.Game;

public static class MainMenuExtensions {

    private const int defaultButtonFontSize = 22;

    public static KButton AddButton(this MainMenu menu, string text, bool highlight, System.Action action) {
        var buttonInfo = new MainMenu.ButtonInfo(
            new LocString(text),
            action,
            defaultButtonFontSize,
            highlight ? menu.topButtonStyle : menu.normalButtonStyle
        );
        return menu.MakeButton(buttonInfo);
    }

}
