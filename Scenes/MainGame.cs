using System;
using System.Collections.Generic;
using Godot;

public partial class MainGame : Node2D
{
	[Export]
	public PackedScene snake_scene;

	private enum Direction
	{
		LEFT,
		RIGHT,
		UP,
		DOWN
	};

	public int score = 0;
	public bool game_start = false;
	public int cells = 20;
	public int cell_size = 50;

	public Vector2 food_pos;
	public bool regen_food = true;

	private Direction _direction;
	private bool can_move = false;
	private List<Panel> body = new List<Panel>();

	private bool _eat = false;

	private bool _crash = false;
	Vector2 start_pos = new Vector2(9, 9);

	public override void _Ready()
	{
		new_game();
	}

	public void new_game()
	{
		GetTree().Paused = false;
		GetTree().Root.GetNode<CanvasLayer>("MainGame/GameOverMenu").Hide();
		GetTree().CallGroup("segments", "queue_free");
		score = 0;
		GetTree()
			.Root
			.GetNode<MainGame>("MainGame")
			.GetNode<Control>("Hud")
			.GetNode<Label>("Score")
			.Text = "SCORE: " + score.ToString();
		_direction = Direction.UP;
		generateSnake();
		move_food();
	}

	public void generateSnake()
	{
		if (body != null)
			body.Clear();
		_direction = Direction.UP;
		can_move = true;

		for (int i = 0; i < 3; i++)
		{
			// take the start_pos and add Vector2(0, i) to it to make the snake body
			GD.Print("Init" + start_pos + " vec " + new Vector2(0, i));
			addSegment((start_pos + new Vector2(0, -i)) * cell_size);
		}
	}

	public void addSegment(Vector2 pos)
	{
		var snakeSegment = snake_scene.Instantiate() as Panel;
		snakeSegment.Position = pos;
		AddChild(snakeSegment);
		body.Insert(0, snakeSegment);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		move_snake();
	}

	public void move_snake()
	{
		if (!can_move)
			return;
		if (
			Input.IsActionJustPressed("move_down")
			& (_direction != Direction.UP | _direction != Direction.DOWN)
		)
		{
			_direction = Direction.DOWN;
			can_move = false;
			if (!game_start)
			{
				gameStart();
			}
		}
		if (
			Input.IsActionJustPressed("move_up")
			& (_direction != Direction.DOWN | _direction != Direction.UP)
		)
		{
			_direction = Direction.UP;
			can_move = false;
			if (!game_start)
			{
				gameStart();
			}
		}
		if (
			Input.IsActionJustPressed("move_left")
			& (_direction != Direction.RIGHT | _direction != Direction.LEFT)
		)
		{
			_direction = Direction.LEFT;
			can_move = false;
			if (!game_start)
			{
				gameStart();
			}
		}
		if (
			Input.IsActionJustPressed("move_right")
			& (_direction != Direction.LEFT || _direction != Direction.RIGHT)
		)
		{
			_direction = Direction.RIGHT;
			can_move = false;
			if (!game_start)
			{
				gameStart();
			}
		}
	}

	public void gameStart()
	{
		game_start = true;
		// get timer
		var timer = GetTree().Root.GetNode<Timer>("MainGame/MoveTimer");
		timer.Start();
	}

	public void _on_move_timer_timeout()
	{
		can_move = true;
		var transition = getDirection(_direction);
		if (body.Count > 0)
		{
			var newSegment = body[0].Position + transition;
			addSegment(newSegment);

			checkOutOfBounds();
			checkSelfEaten();
			bool addBody = checkFoodEaten();

			if (addBody)
			{
				body[body.Count - 1].QueueFree();
				body.RemoveAt(body.Count - 1);
			}
		}
	}

	public void checkOutOfBounds()
	{
		if (
			body[0].Position.X < 0
			|| body[0].Position.X > (cells - 1) * cell_size
			|| body[0].Position.Y < 0
			|| body[0].Position.Y > (cells - 1) * cell_size
		)
		{
			_crash = true;
			gameOver();
		}
	}

	public void checkSelfEaten()
	{
		for (int i = 1; i < body.Count; i++)
		{
			if (body[0].Position == body[i].Position)
			{
				_crash = true;
				gameOver();
			}
		}
	}

	public void move_food()
	{
		while (regen_food)
		{
			regen_food = false;
			food_pos =
				new Vector2((int)GD.RandRange(0, cells - 1), (int)GD.RandRange(0, cells - 1))
					* cell_size
				+ new Vector2(0, cell_size);
			for (int i = 0; i < body.Count; i++)
			{
				if (body[i].Position == food_pos)
				{
					regen_food = true;
					break;
				}
			}
		}
		var food = GetTree().Root.GetNode<Sprite2D>("MainGame/Food");
		food.Position = food_pos; //(food_pos * cell_size) + new Vector2(0, cell_size);
	}

	public bool checkFoodEaten()
	{
		GD.Print("Check food eaten");
		GD.Print(body[0].Position);
		GD.Print(food_pos);
		if (body[0].Position == food_pos)
		{
			GD.Print("Food eaten");
			regen_food = true;
			score += 1;
			GetTree()
				.Root
				.GetNode<MainGame>("MainGame")
				.GetNode<Control>("Hud")
				.GetNode<Label>("Score")
				.Text = "SCORE: " + score.ToString();
			move_food();
			return false;
		}
		return true;
	}

	public void gameOver()
	{
		GD.Print("Trigiger gamer over");
		GetTree().Root.GetNode<CanvasLayer>("MainGame/GameOverMenu").Show();
		GetTree().Root.GetNode<Timer>("MainGame/MoveTimer").Stop();
		game_start = false;
		GetTree().Paused = true;
	}

	private Vector2 getDirection(Direction direction)
	{
		switch (direction)
		{
			case Direction.LEFT:
				return new Vector2(-cell_size, 0);
			case Direction.RIGHT:
				return new Vector2(cell_size, 0);
			case Direction.UP:
				return new Vector2(0, -cell_size);
			case Direction.DOWN:
				return new Vector2(0, cell_size);
			default:
				return new Vector2(0, 0);
		}
	}

	private void _on_game_over_menu_restart()
	{
		new_game();
	}
}
