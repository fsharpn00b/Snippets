using Godot;
using FSharpGodot;

/* Note This class name means the file must be named Player.cs. If the file is named player.cs, the project will build but not run correctly.
*/
public partial class Player : FSharpPlayer {
	[Signal]
	public delegate void HitEventHandler();

/* Godot will not call the inherited methods, so we must override them and then call them explicitly. We do not know why. */
	public override void _Ready () {
		base._Ready ();
	}

	public override void _Process (double delta) {
		base._Process (delta);
	}

	private void body_entered_handler (Node2D body) { base.body_entered_handler (body); }
}
