using UnityEngine;
using System.Collections.ObjectModel;



/// <summary>
/// <see cref="Controls"/> is a set of user defined buttons and axes. It is better to store this file somewhere in your project.
/// </summary>
public static class Controls
{
    /// <summary>
    /// <see cref="Buttons"/> is a set of user defined buttons.
    /// </summary>
    public struct Buttons
    {
        public KeyMapping up;
        public KeyMapping down;
        public KeyMapping left;
        public KeyMapping right;
        public KeyMapping fire1;
        public KeyMapping fire2;
        public KeyMapping fire3;
        public KeyMapping jump;
        public KeyMapping run;
        public KeyMapping lookUp;
        public KeyMapping lookDown;
        public KeyMapping lookLeft;
        public KeyMapping lookRight;
    }

    /// <summary>
    /// <see cref="Axes"/> is a set of user defined axes.
    /// </summary>
    public struct Axes
    {
        public Axis vertical;
        public Axis horizontal;
        public Axis mouseX;
        public Axis mouseY;
    }



	/// <summary>
	/// Set of buttons.
	/// </summary>
    public static Buttons buttons;

	/// <summary>
	/// Set of axes.
	/// </summary>
    public static Axes    axes;



	/// <summary>
	/// Initializes the <see cref="Controls"/> class.
	/// </summary>
    static Controls()
    {
        buttons.up = InputControl.setKey("Up", KeyCode.W, KeyCode.UpArrow, new JoystickInput(JoystickAxis.Axis2Negative));
        buttons.down = InputControl.setKey("Down", KeyCode.S, KeyCode.DownArrow, new JoystickInput(JoystickAxis.Axis2Positive));
        buttons.left = InputControl.setKey("Left", KeyCode.A, KeyCode.LeftArrow, new JoystickInput(JoystickAxis.Axis1Negative));
        buttons.right = InputControl.setKey("Right", KeyCode.D, KeyCode.RightArrow, new JoystickInput(JoystickAxis.Axis1Positive));
        buttons.fire1 = InputControl.setKey("Fire1", MouseButton.Left, KeyCode.LeftControl, new JoystickInput(JoystickButton.Button1));
        buttons.fire2 = InputControl.setKey("Fire2", MouseButton.Right, KeyCode.LeftAlt, new JoystickInput(JoystickButton.Button2));
        buttons.fire3 = InputControl.setKey("Fire3", MouseButton.Middle, KeyCode.LeftCommand, new JoystickInput(JoystickButton.Button3));
        buttons.jump = InputControl.setKey("Jump", KeyCode.Space, KeyCode.None, new JoystickInput(JoystickButton.Button4));
        buttons.run = InputControl.setKey("Run", KeyCode.LeftShift, KeyCode.RightShift, new JoystickInput(JoystickButton.Button5));
        buttons.lookUp = InputControl.setKey("LookUp", MouseAxis.MouseUp, KeyCode.None, new JoystickInput(JoystickAxis.Axis4Negative));
        buttons.lookDown = InputControl.setKey("LookDown", MouseAxis.MouseDown, KeyCode.None, new JoystickInput(JoystickAxis.Axis4Positive));
        buttons.lookLeft = InputControl.setKey("LookLeft", MouseAxis.MouseLeft, KeyCode.None, new JoystickInput(JoystickAxis.Axis3Negative));
        buttons.lookRight = InputControl.setKey("LookRight", MouseAxis.MouseRight, KeyCode.None, new JoystickInput(JoystickAxis.Axis3Positive));

        axes.vertical = InputControl.setAxis("Vertical", buttons.down, buttons.up);
        axes.horizontal = InputControl.setAxis("Horizontal", buttons.left, buttons.right);
        axes.mouseX = InputControl.setAxis("Mouse X", buttons.lookDown, buttons.lookUp);
        axes.mouseY = InputControl.setAxis("Mouse Y", buttons.lookLeft, buttons.lookRight);

        load();
    }

	/// <summary>
	/// Nothing. It just call static constructor if needed.
	/// </summary>
    public static void init()
    {
        // Nothing. It just call static constructor if needed
    }

	/// <summary>
	/// Save controls.
	/// </summary>
    public static void save()
    {
        // It is just an example. You may remove it or modify it if you want
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach(KeyMapping key in keys)
        {
            PlayerPrefs.SetString("Controls." + key.name + ".primary",   key.primaryInput.ToString());
            PlayerPrefs.SetString("Controls." + key.name + ".secondary", key.secondaryInput.ToString());
            PlayerPrefs.SetString("Controls." + key.name + ".third",     key.thirdInput.ToString());
        }

        PlayerPrefs.Save();
    }

	/// <summary>
	/// Load controls.
	/// </summary>
    public static void load()
    {
        // It is just an example. You may remove it or modify it if you want
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach(KeyMapping key in keys)
        {
            string inputStr;

            inputStr = PlayerPrefs.GetString("Controls." + key.name + ".primary");

            if (inputStr != "")
            {
                key.primaryInput = customInputFromString(inputStr);
            }

            inputStr = PlayerPrefs.GetString("Controls." + key.name + ".secondary");

            if (inputStr != "")
            {
                key.secondaryInput = customInputFromString(inputStr);
            }

            inputStr = PlayerPrefs.GetString("Controls." + key.name + ".third");

            if (inputStr != "")
            {
                key.thirdInput = customInputFromString(inputStr);
            }
        }
    }

	/// <summary>
	/// Converts string representation of CustomInput to CustomInput.
	/// </summary>
	/// <returns>CustomInput from string.</returns>
	/// <param name="value">String representation of CustomInput.</param>
    private static CustomInput customInputFromString(string value)
    {
        CustomInput res;

        res = JoystickInput.FromString(value);

        if (res != null)
        {
            return res;
        }

		res = MouseInput.FromString(value);
		
		if (res != null)
		{
			return res;
		}

		res = KeyboardInput.FromString(value);
		
		if (res != null)
		{
			return res;
		}

        return null;
    }
}

