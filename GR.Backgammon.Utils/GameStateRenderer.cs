using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using GR.Gambling.Backgammon;


namespace GR.Gambling.Backgammon.Utils
{
	public class BoundingChequer
	{
		private Rectangle boundingRectangle;
		private int slot; // From the owner's perspective, -1 == finished, 24 == captured

		public BoundingChequer(Rectangle boundingRectangle, int slot)
		{
			this.boundingRectangle = boundingRectangle;
			this.slot = slot;
		}

		public Rectangle BoundingRectangle { get { return boundingRectangle; } }
		public int Slot { get { return slot; } }

		public bool IsHit(Point point)
		{
			if (point.X >= boundingRectangle.Left && point.X <= boundingRectangle.Right && point.Y >= boundingRectangle.Top && point.Y <= boundingRectangle.Bottom)
				return true;

			return false;
		}
	}

    public class GameStateRenderer
    {
        public GameStateRenderer() { }

		private static Bitmap CreateDie(int number, int length, Brush text_brush, Brush bg_brush)
		{
			Bitmap bitmap = new Bitmap(length, length);

			Graphics g = Graphics.FromImage(bitmap);

			//g.Clear(bg_color);
			g.FillRectangle(bg_brush, 0, 0, length, length);

			g.DrawRectangle(Pens.Black, 0, 0, length - 1, length - 1);

			FontFamily fm = new FontFamily("Tahoma");
			Font font = new Font(fm, 14, FontStyle.Bold);

			string text = number.ToString();
			SizeF text_size = g.MeasureString(text, font);

			g.DrawString(text, font, text_brush, new PointF((float)(length / 2.0 - text_size.Width / 2.0), (float)(length / 2.0 - text_size.Height / 2.0)));


			return bitmap;
		}

		public static Bitmap Render(int player, int width, int height, GameState gamestate, ref List<BoundingChequer> bounding_chequers)
		{
			bounding_chequers.Clear();

			int[] board = gamestate.Board.BoardRelativeTo(player);

			int field_width = (int)(0.30 * width);
			int bar_width = (int)(0.08 * width);
			int left_space = (int)(0.16 * width);
			int right_space = (int)(0.16 * width);

			int field_height = (int)(0.92 * height);
			int upper_space = (int)(0.04 * height);
			int lower_space = (int)(0.04 * height);

			int chequer_diameter = (int)(field_width / 6.0);
			int chequer_radius = (int)(chequer_diameter / 2.0);

			int die_width = chequer_diameter;

			int slot_width = chequer_diameter;

			Bitmap bitmap = new Bitmap(width, height);

			Graphics g = Graphics.FromImage(bitmap);

			g.Clear(Color.LightGray);

			Brush[] player_brushes = new Brush[] { Brushes.Brown, Brushes.White };

			int[] slots = new int[24] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

			int base_x = left_space;
			int base_y = upper_space;

			int base_y_down = height - lower_space - chequer_diameter;

			int x = base_x;
			int count = -1, y = -1;

			Brush brush;

			FontFamily fm = new FontFamily("Tahoma");
			Font font = new Font(fm, 17, FontStyle.Bold);

			// Cube
			float cube_y = gamestate.Cube.Centered ? ((float)(height / 2.0 - die_width / 2.0)) : (gamestate.Cube.Owner == 1 ? upper_space : (height - lower_space - die_width));
			g.DrawImageUnscaled(GameStateRenderer.CreateDie(gamestate.Cube.Value, die_width, Brushes.Black, Brushes.White), (int)(left_space / 2.0 - die_width / 2.0), (int)cube_y);

			// Finished counts		
			string finished_text = gamestate.Board.FinishedCount(1).ToString();
			SizeF text_size = g.MeasureString(finished_text, font);

			if (gamestate.Board.FinishedCount(1) > 0)
				g.DrawString(finished_text, font, Brushes.White, new PointF((float)(width - right_space / 2.0 - text_size.Width / 2.0), base_y));

			finished_text = gamestate.Board.FinishedCount(0).ToString();
			text_size = g.MeasureString(finished_text, font);

			if (gamestate.Board.FinishedCount(0) > 0)
				g.DrawString(finished_text, font, Brushes.Brown, new PointF((float)(width - right_space / 2.0 - text_size.Width / 2.0), (float)(height - lower_space - text_size.Height)));

			for (int s = 0; s < 12; s++)
			{
				int tri_x = x - ((slot_width / 2) - chequer_radius);
				int tri_y_down = upper_space + (int)(6 * chequer_diameter);
				int try_y_down2 = height - lower_space;//left_space + (int)(5.5 * chequer_diameter) + (int)(5.5 * chequer_diameter);

				Brush triangle_brush = (s % 2 == 0) ? Brushes.DarkGray : Brushes.White;

				g.FillPolygon(triangle_brush, new Point[] { new Point(tri_x, base_y), new Point(tri_x + slot_width, base_y), new Point(tri_x + slot_width / 2, base_y + 5 * chequer_diameter) });
				g.FillPolygon(triangle_brush, new Point[] { new Point(tri_x, try_y_down2 + 1), new Point(tri_x + slot_width / 2, tri_y_down), new Point(tri_x + slot_width, try_y_down2 + 1) });

				/*string text = (slots[s] + 1).ToString();
				SizeF size = g.MeasureString(text, font);
				float text_x = (float)(x + chequer_radius) - size.Width / 2.0f;
				g.DrawString(text, font, Brushes.Blue, new PointF(text_x, left_space - size.Height));

				text = (slots[12 + s] + 1).ToString();
				size = g.MeasureString(text, font);
				text_x = (float)(x + chequer_radius) - size.Width / 2.0f;
				g.DrawString(text, font, Brushes.Blue, new PointF(text_x, try_y_down2 + 2));*/

				// render up
				count = board[slots[s]];
				y = base_y;
				//int player = (count > 0) ? 0 : 1;//board[slots[s]].Player;
				brush = player_brushes[(count > 0) ? 0 : 1];
				bool hero = (count > 0) ? true : false;
				if (count < 0) count *= -1;
				for (int c = 0; c < count; c++)
				{
					if (c % 5 == 0)
						y = base_y + chequer_radius;
					if (c % 10 == 0)
						y = base_y;

					g.FillEllipse(brush, x, y, chequer_diameter, chequer_diameter);
					g.DrawEllipse(Pens.Black, x, y, chequer_diameter, chequer_diameter);

					if (hero) 
						bounding_chequers.Add(new BoundingChequer(new Rectangle(x, y, chequer_diameter, chequer_diameter), slots[s]));

					y += chequer_diameter;
				}

				// render down
				count = board[slots[12 + s]];
				y = base_y_down;
				//player = (count > 0) ? 0 : 1;
				brush = player_brushes[(count > 0) ? 0 : 1];
				hero = (count > 0) ? true : false;
				if (count < 0) count *= -1;
				for (int c = 0; c < count; c++)
				{
					if (c % 5 == 0)
						y = base_y_down - chequer_radius;
					if (c % 10 == 0)
						y = base_y_down;

					g.FillEllipse(brush, x, y, chequer_diameter, chequer_diameter);
					g.DrawEllipse(Pens.Black, x, y, chequer_diameter, chequer_diameter);

					if (hero)
						bounding_chequers.Add(new BoundingChequer(new Rectangle(x, y, chequer_diameter, chequer_diameter), slots[12 + s]));

					y -= chequer_diameter;
				}

				x += slot_width;

				if (s == 5)
					x = left_space + 6 * slot_width + bar_width + (slot_width / 2) - chequer_radius;
			}

			// Captured
			x = (int)(width / 2.0) - chequer_radius;

			// Player's
			count = gamestate.Board.CapturedCount(player);
			y = base_y + 4 * chequer_diameter;

			brush = player_brushes[0];
			for (int c = 0; c < count; c++)
			{
				if (c % 5 == 0)
					y = base_y + 4 * chequer_diameter - chequer_radius;
				if (c % 10 == 0)
					y = base_y + 4 * chequer_diameter;
				
				g.FillEllipse(brush, x, y, chequer_diameter, chequer_diameter);
				g.DrawEllipse(Pens.Black, x, y, chequer_diameter, chequer_diameter);

				bounding_chequers.Add(new BoundingChequer(new Rectangle(x, y, chequer_diameter, chequer_diameter), 24));

				y -= chequer_diameter;
			}

			// Opponent's
			count = gamestate.Board.CapturedCount(1 - player);
			y = height - lower_space - 5 * chequer_diameter;

			brush = player_brushes[1];
			for (int c = 0; c < count; c++)
			{
				if (c % 5 == 0)
					y = height - lower_space - 5 * chequer_diameter + chequer_radius;
				if (c % 10 == 0)
					y = height - lower_space - 5 * chequer_diameter;

				g.FillEllipse(brush, x, y, chequer_diameter, chequer_diameter);
				g.DrawEllipse(Pens.Black, x, y, chequer_diameter, chequer_diameter);

				// bounding_chequers.Add(new BoundingChequer(new Rectangle(x, y, chequer_diameter, chequer_diameter), 25));

				y += chequer_diameter;
			}

			// Dice
			/*SizeF size; string text;
			int[] dice = gamestate.Dice;
			if (dice[0] != 0)
			{
				text = dice[0].ToString() + "," + dice[1].ToString();
				size = g.MeasureString(text, font);

				g.DrawString(text, font, Brushes.Blue, new PointF(left_space + 3 * slot_width - size.Width / 2, height / 2 - size.Height / 2));
			}*/

			if (gamestate.DiceRolled)
			{
				int[] dice_x = new int[2];
				if (player == gamestate.PlayerOnRoll)
				{
					dice_x[0] = (int)(left_space + field_width * 0.25 - die_width * 0.5);
					dice_x[1] = (int)(left_space + field_width * 0.75 - die_width * 0.5);
				}
				else
				{
					dice_x[0] = (int)(left_space + field_width + bar_width + field_width * 0.25 - die_width * 0.5);
					dice_x[1] = (int)(left_space + field_width + bar_width + field_width * 0.75 - die_width * 0.5);
				}

				g.DrawImageUnscaled(CreateDie(gamestate.Dice[0], die_width, player_brushes[gamestate.PlayerOnRoll], player_brushes[1 - gamestate.PlayerOnRoll]), dice_x[0], (int)(height * 0.5 - die_width * 0.5));
				g.DrawImageUnscaled(CreateDie(gamestate.Dice[1], die_width, player_brushes[gamestate.PlayerOnRoll], player_brushes[1 - gamestate.PlayerOnRoll]), dice_x[1], (int)(height * 0.5 - die_width * 0.5));
			}

			// Outer borders
			g.DrawRectangle(Pens.Black, 0, 0, width - 1, height - 1);

			// Fields
			g.DrawRectangle(Pens.Black, left_space, upper_space, field_width, field_height); // Left
			g.DrawRectangle(Pens.Black, width - right_space - field_width, upper_space, field_width, field_height); // Right

			return bitmap;
		}

        /// <summary>
        /// Renders relative to the player's perspective.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gamestate"></param>
        /// <returns></returns>
        public static Bitmap Render(int player, GameState gamestate)
        {
            int[] board = gamestate.Board.BoardRelativeTo(player);

            int cheq_rad = 6;
            int slot_width = 18, bar_width = 15, border_width = 20, middle_height = 8;
            int cheq_diam = cheq_rad * 2;

            int total_width = 2 * border_width + 12 * slot_width + bar_width + 1;
            int total_height = 2 * border_width + 2 * (int)(5.5 * cheq_diam) + middle_height + 1;
            Bitmap render = new Bitmap(total_width, total_height);

            //int[] slots = new int[24] {0,1,2,3,4,5,6,7,8,9,10,11,23,22,21,20,19,18,17,16,15,14,13,12};
            int[] slots = new int[24] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

            int base_x = border_width + (slot_width / 2) - cheq_rad;
            int base_y = border_width;

            int base_y_down = border_width + (int)(5.5 * cheq_diam) + middle_height + (int)(4.5 * cheq_diam);

            Graphics g = Graphics.FromImage(render);
            g.Clear(Color.LightGray);
            g.DrawRectangle(Pens.Turquoise, 0, 0, total_width - 1, total_height - 1);
            g.DrawRectangle(Pens.DarkGray, border_width - 1, border_width - 1, slot_width * 6 + 2, cheq_diam * 11 + middle_height + 2);
            g.DrawRectangle(Pens.DarkGray, border_width + slot_width * 6 + bar_width - 1, border_width - 1, slot_width * 6 + 2, cheq_diam * 11 + middle_height + 2);

            Dictionary<int, Brush> player2brush = new Dictionary<int, Brush>();
            player2brush[0] = Brushes.Brown;
            player2brush[1] = Brushes.White;

            int x = base_x;

            FontFamily fm = new FontFamily("Tahoma");
            Font font = new Font(fm, 7, FontStyle.Bold);

            // cube
            string cube_text = gamestate.Cube.Value.ToString();
            SizeF text_size = g.MeasureString(cube_text, font);

            g.DrawString(cube_text, font, Brushes.Blue, new PointF(border_width / 2 - text_size.Width / 2, total_height / 2 - text_size.Height / 2));

            // finished counts
            /*string finished_text = "C:{" + gamestate.Board.CapturedCount(0) + ", " + gamestate.Board.CapturedCount(1) + "}";
            finished_text += " F:{" + gamestate.Board.FinishedCount(0) + ", " + gamestate.Board.FinishedCount(1) + "}";
            SizeF text_size = g.MeasureString(finished_text, font);*/

            
            for (int s = 0; s < 12; s++)
            {
                int tri_x = x - ((slot_width / 2) - cheq_rad);
                int tri_y_down = border_width + (int)(6 * cheq_diam) + middle_height;
                int try_y_down2 = border_width + (int)(5.5 * cheq_diam) + middle_height + (int)(5.5 * cheq_diam);
                g.FillPolygon(Brushes.LightGoldenrodYellow, new Point[] { new Point(tri_x, base_y), new Point(tri_x + slot_width, base_y), new Point(tri_x + slot_width / 2, base_y + 5 * cheq_diam) });
                g.FillPolygon(Brushes.LightGoldenrodYellow, new Point[] { new Point(tri_x, try_y_down2 + 1), new Point(tri_x + slot_width / 2, tri_y_down), new Point(tri_x + slot_width, try_y_down2 + 1) });

                string text = (slots[s] + 1).ToString();
                SizeF size = g.MeasureString(text, font);
                float text_x = (float)(x + cheq_rad) - size.Width / 2.0f;
                g.DrawString(text, font, Brushes.Blue, new PointF(text_x, border_width - size.Height));

                text = (slots[12 + s] + 1).ToString();
                size = g.MeasureString(text, font);
                text_x = (float)(x + cheq_rad) - size.Width / 2.0f;
                g.DrawString(text, font, Brushes.Blue, new PointF(text_x, try_y_down2 + 2));

                int[] dice = gamestate.Dice;
                if (dice[0] != 0)
                {
                    text = dice[0].ToString() + "," + dice[1].ToString();
                    size = g.MeasureString(text, font);

                    g.DrawString(text, font, Brushes.Blue, new PointF(border_width + 3 * slot_width - size.Width / 2, total_height / 2 - size.Height / 2));
                }

                // render up
                int count = board[slots[s]];
                int y = base_y;
                //int player = (count > 0) ? 0 : 1;//board[slots[s]].Player;
                Brush brush = player2brush[(count > 0) ? 0 : 1];
                if (count < 0) count *= -1;
                for (int c = 0; c < count; c++)
                {
                    if (c % 5 == 0)
                        y = base_y + cheq_rad;
                    if (c % 10 == 0)
                        y = base_y;

                    g.FillEllipse(brush, x, y, cheq_diam, cheq_diam);
                    g.DrawEllipse(Pens.Black, x, y, cheq_diam, cheq_diam);

                    y += cheq_diam;
                }

                // render down
                count = board[slots[12 + s]];
                y = base_y_down;
                //player = (count > 0) ? 0 : 1;
                brush = player2brush[(count > 0) ? 0 : 1];
                if (count < 0) count *= -1;
                for (int c = 0; c < count; c++)
                {
                    if (c % 5 == 0)
                        y = base_y_down - cheq_rad;
                    if (c % 10 == 0)
                        y = base_y_down;

                    g.FillEllipse(brush, x, y, cheq_diam, cheq_diam);
                    g.DrawEllipse(Pens.Black, x, y, cheq_diam, cheq_diam);

                    y -= cheq_diam;
                }

                x += slot_width;

                if (s == 5)
                    x = border_width + 6 * slot_width + bar_width + (slot_width / 2) - cheq_rad;
            }
            
            return render;
        }

        public static Bitmap Render(GameState gamestate)
        {
            Board board = gamestate.Board;
            int cheq_rad = 6;
            int slot_width = 18, bar_width = 15, border_width = 20, middle_height = 8;
            int cheq_diam = cheq_rad * 2;

            int total_width = 2 * border_width + 12 * slot_width + bar_width + 1;
            int total_height = 2 * border_width + 2 * (int)(5.5 * cheq_diam) + middle_height + 1;
            Bitmap render = new Bitmap(total_width, total_height);

            //int[] slots = new int[24] {0,1,2,3,4,5,6,7,8,9,10,11,23,22,21,20,19,18,17,16,15,14,13,12};
            int[] slots = new int[24] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

            int base_x = border_width + (slot_width / 2) - cheq_rad;
            int base_y = border_width;

            int base_y_down = border_width + (int)(5.5 * cheq_diam) + middle_height + (int)(4.5 * cheq_diam);

            Graphics g = Graphics.FromImage(render);
            g.Clear(Color.LightGray);
            g.DrawRectangle(Pens.Turquoise, 0, 0, total_width - 1, total_height - 1);
            g.DrawRectangle(Pens.DarkGray, border_width - 1, border_width - 1, slot_width * 6 + 2, cheq_diam * 11 + middle_height + 2);
            g.DrawRectangle(Pens.DarkGray, border_width + slot_width * 6 + bar_width - 1, border_width - 1, slot_width * 6 + 2, cheq_diam * 11 + middle_height + 2);

            Dictionary<int, Brush> player2brush = new Dictionary<int, Brush>();
            player2brush[0] = Brushes.Brown;
            player2brush[1] = Brushes.White;

            int x = base_x;

            FontFamily fm = new FontFamily("Tahoma");
            Font font = new Font(fm, 7, FontStyle.Bold);

            // cube
            string cube_text = gamestate.Cube.Value.ToString();
            SizeF text_size = g.MeasureString(cube_text, font);

            g.DrawString(cube_text, font, Brushes.Blue, new PointF(border_width / 2 - text_size.Width / 2, total_height / 2 - text_size.Height / 2));

            // finished counts
            /*string finished_text = "C:{" + gamestate.Board.CapturedCount(0) + ", " + gamestate.Board.CapturedCount(1) + "}";
            finished_text += " F:{" + gamestate.Board.FinishedCount(0) + ", " + gamestate.Board.FinishedCount(1) + "}";
            SizeF text_size = g.MeasureString(finished_text, font);*/

            /*
            for (int s = 0; s < 12; s++)
            {
                int tri_x = x - ((slot_width / 2) - cheq_rad);
                int tri_y_down = border_width + (int)(6 * cheq_diam) + middle_height;
                int try_y_down2 = border_width + (int)(5.5 * cheq_diam) + middle_height + (int)(5.5 * cheq_diam);
                g.FillPolygon(Brushes.LightGoldenrodYellow, new Point[] { new Point(tri_x, base_y), new Point(tri_x + slot_width, base_y), new Point(tri_x + slot_width / 2, base_y + 5 * cheq_diam) });
                g.FillPolygon(Brushes.LightGoldenrodYellow, new Point[] { new Point(tri_x, try_y_down2 + 1), new Point(tri_x + slot_width / 2, tri_y_down), new Point(tri_x + slot_width, try_y_down2 + 1) });

                string text = (slots[s] + 1).ToString();
                SizeF size = g.MeasureString(text, font);
                float text_x = (float)(x + cheq_rad) - size.Width / 2.0f;
                g.DrawString(text, font, Brushes.Blue, new PointF(text_x, border_width - size.Height));

                text = (slots[12 + s] + 1).ToString();
                size = g.MeasureString(text, font);
                text_x = (float)(x + cheq_rad) - size.Width / 2.0f;
                g.DrawString(text, font, Brushes.Blue, new PointF(text_x, try_y_down2 + 2));

                int[] dice = gamestate.Dice;
                if (dice[0] != 0)
                {
                    text = dice[0].ToString() + "," + dice[1].ToString();
                    size = g.MeasureString(text, font);

                    g.DrawString(text, font, Brushes.Blue, new PointF(border_width + 3 * slot_width - size.Width / 2, total_height / 2 - size.Height / 2));
                }

                // render up
                int count = board.Count(slots[s]);
                int y = base_y;
                int player = board[slots[s]].Player;
                for (int c = 0; c < count; c++)
                {
                    if (c % 5 == 0)
                        y = base_y + cheq_rad;
                    if (c % 10 == 0)
                        y = base_y;

                    g.FillEllipse(player2brush[player], x, y, cheq_diam, cheq_diam);
                    g.DrawEllipse(Pens.Black, x, y, cheq_diam, cheq_diam);

                    y += cheq_diam;
                }

                // render down
                count = board.Count(slots[12 + s]);
                y = base_y_down;
                player = board[slots[12 + s]].Player;
                for (int c = 0; c < count; c++)
                {
                    if (c % 5 == 0)
                        y = base_y_down - cheq_rad;
                    if (c % 10 == 0)
                        y = base_y_down;

                    g.FillEllipse(player2brush[player], x, y, cheq_diam, cheq_diam);
                    g.DrawEllipse(Pens.Black, x, y, cheq_diam, cheq_diam);

                    y -= cheq_diam;
                }

                x += slot_width;

                if (s == 5)
                    x = border_width + 6 * slot_width + bar_width + (slot_width / 2) - cheq_rad;
            }
            */
            return render;
        }
    }
}
