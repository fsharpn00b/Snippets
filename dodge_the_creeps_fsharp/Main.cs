using FSharpGodot;

/* TODO2 If these are partial classes, where are the rest of the class definitions? */
public partial class Main : FSharpMain {
/* Godot will not call the inherited methods, so we must override them and then call them explicitly. We do not know why. */
	new private void hud_start_game_handler () { base.hud_start_game_handler () ; }
	new private void hud_show_attract_screen_handler () { base.hud_show_attract_screen_handler (); }
	new private void player_hit_handler () { base.player_hit_handler (); }
	new private void enemy_timer_timeout_handler () { base.enemy_timer_timeout_handler (); }
	new private void score_timer_timeout_handler () { base.score_timer_timeout_handler (); }
	new private void start_timer_timeout_handler () { base.start_timer_timeout_handler (); }
}
