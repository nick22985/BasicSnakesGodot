using System;
using Godot;

public partial class game_over_menu : CanvasLayer
{
	[Signal]
	public delegate void restartEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() { }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }

	public void _on_restart_button_pressed()
	{
		EmitSignal(SignalName.restart);
	}
}
