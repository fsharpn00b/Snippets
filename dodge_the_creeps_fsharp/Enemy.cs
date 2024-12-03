using FSharpGodot;

public partial class Enemy : FSharpEnemy {
/* Godot will not call the inherited methods, so we must override them and then call them explicitly. We do not know why. */
	public override void _Ready () {
		base._Ready ();
	}

	new private void visible_on_screen_notifier_2d_enemy_screen_exited_handler () { base.visible_on_screen_notifier_2d_enemy_screen_exited_handler (); }
}
