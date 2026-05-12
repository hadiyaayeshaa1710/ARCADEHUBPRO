using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ArcadeFinalFix
{
	static class Theme
	{
		public static readonly Color Bg = Color.FromArgb(6, 8, 20);
		public static readonly Color BgCard = Color.FromArgb(14, 16, 34);
		public static readonly Color BgPanel = Color.FromArgb(10, 12, 26);
		public static readonly Color Neon = Color.FromArgb(0, 230, 140);
		public static readonly Color Blue = Color.FromArgb(60, 160, 255);
		public static readonly Color Purple = Color.FromArgb(160, 80, 255);
		public static readonly Color Pink = Color.FromArgb(255, 60, 180);
		public static readonly Color Gold = Color.FromArgb(255, 200, 50);
		public static readonly Color TextDim = Color.FromArgb(110, 130, 160);
		public static readonly Color TextBright = Color.FromArgb(210, 225, 255);

		public static void DrawGlowBorder(Graphics g, Rectangle rect, Color accent, int radius = 8)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			for (int i = 4; i >= 1; i--)
				using (var pen = new Pen(Color.FromArgb(16 * i, accent), i * 2 + 1))
					DrawRoundRect(g, pen, rect.X - i, rect.Y - i,
								  rect.Width + i * 2, rect.Height + i * 2, radius + i);
			using (var pen = new Pen(Color.FromArgb(200, accent), 1.5f))
				DrawRoundRect(g, pen, rect.X, rect.Y, rect.Width, rect.Height, radius);
		}

		public static void DrawRoundRect(Graphics g, Pen pen, int x, int y, int w, int h, int r)
		{
			using (var path = RoundRectPath(x, y, w, h, r))
				g.DrawPath(pen, path);
		}

		public static void FillRoundRect(Graphics g, Brush br, int x, int y, int w, int h, int r)
		{
			using (var path = RoundRectPath(x, y, w, h, r))
				g.FillPath(br, path);
		}

		public static GraphicsPath RoundRectPath(int x, int y, int w, int h, int r)
		// since static is used, you dont need to create obj to use this,Theme.RoundRectPath(...)works dir
		{
			r = Math.Max(1, Math.Min(r, Math.Min(w, h) / 2));
			//Ensures: radius is at least 1,radius is not bigger than half the rectangle size
			var p = new GraphicsPath();
			// x,y are arc position, r*2 is circle size, S.A ,E.A (angle)
			p.AddArc(x, y, r * 2, r * 2, 180, 90);
			//top right arc
			p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
			//bottom right arc
			p.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
			//bottom left arc
			p.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
			p.CloseFigure();
			return p;
			//method is used later i draw round rect and fill round rect
		}
	}

	static class IntExt
	{
		public static int Clamp(this int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
		//this int v converts the method into an extension method so it can be called like a built-in function on integers.
		//An extension method adds custom functionality to existing data types without modifying the original class.
		//clamp is used to restrict a value within a minimum and maximum range.
		//Used mainly for safe RGB color values.(lo=min,hi=max)
		public static double Clamp(this double v, double lo, double hi) => v < lo ? lo : v > hi ? hi : v;
		//method ovrload hpnd bcz sem parameter name, diff data-type
		//Abstraction is used here because the complex logic of limiting values is hidden inside the Clamp() method,
		//and the rest of the program only uses the simple method call.
	}

	public class FrontPage : Form
	{
		Timer animTimer = new Timer { Interval = 40 };
		int pulse = 0;
		Label lblTitle;

		struct Star { public float x, y, speed, size; public int alpha; }
		List<Star> stars = new List<Star>();
		Random rnd = new Random(42);

		public FrontPage()
		{
			this.Text = "ARCADE HUB PRO";
			this.Size = new Size(1100, 800);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.BackColor = Theme.Bg;
			this.DoubleBuffered = true;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MaximizeBox = true;

			for (int i = 0; i < 200; i++)
				stars.Add(new Star
				{
					x = (float)(rnd.NextDouble() * 1100),
					y = (float)(rnd.NextDouble() * 800),
					speed = (float)(rnd.NextDouble() * 0.35 + 0.05),
					size = (float)(rnd.NextDouble() * 2.2 + 0.5),
					alpha = rnd.Next(50, 200)
				});

			BuildUI();

			animTimer.Tick += (s, e) =>
			{
				pulse = (pulse + 1) % 120;
				//resets the pulse counter after 120 to create a repeating animation cycl
				double t = Math.Sin(pulse * Math.PI / 60.0);
				//Math.Sin() creates smooth wave motion.
				//div by 60 Converts pulse into radians for sine function
				int rr = ((int)(0 + t * 45)).Clamp(0, 255);
				int gg = ((int)(230 + t * 25)).Clamp(0, 255);
				int bb = ((int)(140 + t * 90)).Clamp(0, 255);
				if (lblTitle != null)
					lblTitle.ForeColor = Color.FromArgb(rr, gg, bb);
				//This code creates the animated glowing color effect

				for (int i = 0; i < stars.Count; i++)
				{
					var st = stars[i];
					st.y -= st.speed;
					if (st.y < -2) st.y = this.ClientSize.Height + 2;
					stars[i] = st;
					//star uper jakar ded hokr neeche se phr spawn hote
				}
				this.Invalidate(); //Invalidate() forces the form to repaint itself so updated star positions become visible on screen
			};
			animTimer.Start(); //This is frame-based real-time animation controlled by a timer event.
		}

		protected override void OnPaint(PaintEventArgs e) // ovr Replace the default form painting with custom drawing.
		{
			base.OnPaint(e); //OnPaint() is a built-in Windows Forms method that runs whenever the form needs to redraw itself.
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias; //antiAlias smooths graphical edges and improves visual quality by reducing jagged lines.

			foreach (var st in stars)
				using (var br = new SolidBrush(Color.FromArgb(st.alpha, 180, 210, 255)))
					g.FillEllipse(br, st.x, st.y, st.size, st.size); //FillEllipse creates small circular star particles for the animated background.

			using (var br = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
				for (int y = 0; y < this.ClientSize.Height; y += 3)
					g.FillRectangle(br, 0, y, this.ClientSize.Width, 1);

			int vh = this.ClientSize.Height;
			using (var lgb = new LinearGradientBrush(
				new Point(0, vh - 220), new Point(0, vh),
				Color.Transparent, Color.FromArgb(200, Theme.Bg)))
				g.FillRectangle(lgb, 0, vh - 220, this.ClientSize.Width, 220);
		}

		private void BuildUI()
		{
			lblTitle = new Label
			{
				Text = "◈  ARCADE  HUB  PRO  ◈",
				Font = new Font("Consolas", 34, FontStyle.Bold),
				ForeColor = Theme.Neon,
				BackColor = Color.Transparent,
				AutoSize = false,
				Dock = DockStyle.None,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Left = 0,
				Top = 55,
				Width = 1100,
				Height = 90,
				TextAlign = ContentAlignment.MiddleCenter
			};
			this.Controls.Add(lblTitle);
			this.Resize += (s, e) => { if (lblTitle != null) lblTitle.Width = this.ClientSize.Width; };

			var lbSub = new Label
			{
				Text = "S E L E C T   Y O U R   M I S S I O N",
				Font = new Font("Consolas", 12),
				ForeColor = Theme.Blue,
				BackColor = Color.Transparent,
				AutoSize = false,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Left = 0,
				Top = 155,
				Width = 1100,
				Height = 32,
				TextAlign = ContentAlignment.MiddleCenter
			};
			this.Controls.Add(lbSub);
			this.Resize += (s, e) => { lbSub.Width = this.ClientSize.Width; };

			var div = new Panel
			{
				Height = 1,
				BackColor = Color.FromArgb(55, Theme.Blue),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Left = 200,
				Top = 200,
				Width = 700
			};
			this.Controls.Add(div);
			this.Resize += (s, e) => {
				div.Left = this.ClientSize.Width / 2 - 350;
				div.Width = 700;
			};

			string[] names = { "WORD SEARCH", "MEMORY MATCH", "BALLOON POP" };
			string[] icons = { "[ W ]", "[ M ]", "[ B ]" };
			string[] descs = {
				"Hunt hidden words\nthrough the letter grid.\nTrack them down!",
				"Flip cards & match\nidentical pairs.\nTest your memory!",
				"Pop the rising\nballoons before they\nfloat away. Aim!"
			};
			string[] btns = { "PLAY WORD SEARCH", "PLAY MEMORY", "PLAY BALLOONS" };
			Color[] accents = { Theme.Neon, Theme.Purple, Theme.Pink };

			var cardPanels = new Panel[3];
			for (int i = 0; i < 3; i++)
			{
				int id = i;
				var card = MakeCard(names[i], icons[i], descs[i], btns[i], accents[i], id);
				card.Top = 228;
				card.Left = 60 + i * 330; //It spaces cards evenly by shifting each card horizontally based on its index
				card.Anchor = AnchorStyles.Top;
				this.Controls.Add(card);
				cardPanels[i] = card;
			}
			this.Resize += (s, e) => {
				int totalW = 3 * 300 + 2 * 30;
				int startX = (this.ClientSize.Width - totalW) / 2;
				for (int i = 0; i < 3; i++)
					cardPanels[i].Left = startX + i * 330;
			};

			var btnAll = MakeNeonButton("▶  PLAY ALL GAMES", Theme.Gold);
			btnAll.Top = 632; btnAll.Width = 300; btnAll.Height = 52;
			btnAll.Left = (1100 - 300) / 2;
			btnAll.Font = new Font("Consolas", 12, FontStyle.Bold);
			btnAll.Anchor = AnchorStyles.Top;
			btnAll.Click += (s, e) => LaunchGame(0);
			this.Controls.Add(btnAll);
			this.Resize += (s, e) => { btnAll.Left = (this.ClientSize.Width - btnAll.Width) / 2; };

			var footer = new Label
			{
				Text = "◆  INSERT COIN TO CONTINUE  ◆",
				Font = new Font("Consolas", 9),
				ForeColor = Color.FromArgb(45, 80, 60),
				BackColor = Color.Transparent,
				AutoSize = false,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				Left = 0,
				Top = 734,
				Width = 1100,
				Height = 26,
				TextAlign = ContentAlignment.MiddleCenter
			};
			this.Controls.Add(footer);
			this.Resize += (s, e) => {
				footer.Width = this.ClientSize.Width;
				footer.Top = this.ClientSize.Height - 46;
			};
		}

		private Panel MakeCard(string name, string icon, string desc,
								string btnLabel, Color accent, int gameId)
		{
			var card = new Panel
			{
				Width = 300,
				Height = 368,
				BackColor = Theme.BgCard,
				Cursor = Cursors.Hand
			};
			bool hov = false;

			card.Paint += (s, e) => {
				var g = e.Graphics;
				g.SmoothingMode = SmoothingMode.AntiAlias;

				using (var br = new SolidBrush(hov ? Color.FromArgb(22, 26, 50) : Theme.BgCard))
					Theme.FillRoundRect(g, br, 0, 0, card.Width, card.Height, 10);

				Theme.DrawGlowBorder(g,
					new Rectangle(2, 2, card.Width - 5, card.Height - 5),
					hov ? accent : Color.FromArgb(70, accent.R, accent.G, accent.B), 10);

				using (var lgb = new LinearGradientBrush(
					new Point(0, 0), new Point(card.Width, 0),
					Color.Transparent, Color.FromArgb(hov ? 90 : 45, accent)))
					Theme.FillRoundRect(g, lgb, 2, 2, card.Width - 5, 5, 3);

				using (var f = new Font("Consolas", 26, FontStyle.Bold))
				using (var br = new SolidBrush(accent))
				{
					var sz = g.MeasureString(icon, f);
					g.DrawString(icon, f, br, (card.Width - sz.Width) / 2f, 26f);
				}
				using (var f = new Font("Consolas", 14, FontStyle.Bold))
				using (var br = new SolidBrush(Theme.TextBright))
				{
					var sz = g.MeasureString(name, f); //Calculates how wide the text will be when drawn.MeasureString is used to calculate text width so the game title can be centered inside the card.
					g.DrawString(name, f, br, (card.Width - sz.Width) / 2f, 100f);//Draws the game name centered horizontally.
					g.DrawString(name, f, br, (card.Width - sz.Width) / 2f, 100f);
				}
				using (var f = new Font("Consolas", 9))
				using (var br = new SolidBrush(Theme.TextDim))
				using (var sf = new StringFormat { Alignment = StringAlignment.Center }) //This aligns text to center horizontally.
					g.DrawString(desc, f, br, new RectangleF(16, 138, card.Width - 32, 82), sf);//Draws multi-line description inside a box(h=82)

				using (var pen = new Pen(Color.FromArgb(38, accent), 1)) //Creates a thin colored line.
					g.DrawLine(pen, 20, 234, card.Width - 20, 234); //Draws horizontal separator line inside card.
			};

			card.MouseEnter += (s, e) => { hov = true; card.Invalidate(); }; //Invalidate forces the control to redraw itself so visual changes like hover effects appear immediately.
			card.MouseLeave += (s, e) => { hov = false; card.Invalidate(); };

			var btn = MakeNeonButton(btnLabel, accent);
			btn.Width = 224; btn.Height = 42;
			btn.Left = (300 - 224) / 2; btn.Top = 252;
			btn.Click += (s, e) => LaunchGame(gameId); //opens selected game
			btn.MouseEnter += (s, e) => { hov = true; card.Invalidate(); };
			btn.MouseLeave += (s, e) => { hov = false; card.Invalidate(); };
			card.Controls.Add(btn);

			return card;
		}

		private Button MakeNeonButton(string text, Color accent)
		{
			var b = new Button
			{
				Text = text,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(18, accent.R, accent.G, accent.B),
				ForeColor = accent,
				Font = new Font("Consolas", 10, FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			b.FlatAppearance.BorderColor = Color.FromArgb(160, accent);
			b.FlatAppearance.BorderSize = 1;
			b.MouseEnter += (s, e) => {
				b.BackColor = Color.FromArgb(55, accent.R, accent.G, accent.B);
				b.FlatAppearance.BorderColor = accent;
			};
			b.MouseLeave += (s, e) => {
				b.BackColor = Color.FromArgb(18, accent.R, accent.G, accent.B);
				b.FlatAppearance.BorderColor = Color.FromArgb(160, accent);
			};
			return b;
		}

		private void LaunchGame(int startId)
		{
			animTimer.Stop();
			this.Hide();
			var arcade = new ArcadeForm(startId);
			arcade.FormClosed += (s, e) => { animTimer.Start(); this.Show(); };
			arcade.Show();
		}

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FrontPage());
		}
	}

	public class ArcadeForm : Form
	{
		Panel pnlGame = new Panel();
		Panel pnlNav = new Panel();
		Panel pnlWordBank = new Panel();
		Label lblStats = new Label();
		Timer globalClock = new Timer { Interval = 1000 };

		int totalSeconds = 240, score = 0;
		int balloonsPopped = 0, memoryMatches = 0;
		bool wordSearchDone = false, memoryDone = false, balloonsDone = false;

		string currentInput = "";
		List<Button> selectedButtons = new List<Button>();
		List<string> targets = new List<string> { "SPACE", "ATOM", "STARS", "MOON", "CELL" };
		List<WordEntry> wordEntries = new List<WordEntry>();
		Button[,] gridButtons = new Button[10, 10];
		int lastSelRow = -1, lastSelCol = -1;
		List<Label> wbLabels = new List<Label>();

		Button memFirst;

		public ArcadeForm(int startId = 0)
		{
			this.Text = "Arcade Hub Pro";
			this.Size = new Size(1140, 820);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.BackColor = Theme.Bg;
			this.DoubleBuffered = true;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MaximizeBox = true;

			InitShell();
			SwitchGame(startId);

			globalClock.Tick += (s, e) => //Tick event har fixed time interval ke baad repeatedly run hota hai.
			{
				totalSeconds--;
				RefreshStats();
				if (totalSeconds <= 0)
				{
					globalClock.Stop();
					MessageBox.Show("TIME IS UP!", "System Alert");
					this.Close();
				}
			};
			globalClock.Start();
		}

		private void InitShell() //nitShell method UI ka basic structure create karta hai jisme top bar aur stats display setup hota ha
		{
			var topBar = new Panel //Ek panel bana rahe hain jo top bar ka kaam karega
			{
				Dock = DockStyle.Top,
				Height = 56,
				BackColor = Color.FromArgb(10, 12, 28)
			};
			topBar.Paint += (s, e) => {
				using (var pen = new Pen(Color.FromArgb(40, Theme.Neon), 1))
					e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
			};
			lblStats.Dock = DockStyle.Fill;
			lblStats.ForeColor = Theme.Neon;
			lblStats.Font = new Font("Consolas", 14, FontStyle.Bold);
			lblStats.TextAlign = ContentAlignment.MiddleLeft;
			lblStats.BackColor = Color.Transparent;
			topBar.Controls.Add(lblStats);
			this.Controls.Add(topBar);

			pnlNav = new Panel
			{
				Dock = DockStyle.Left,
				Width = 150,
				BackColor = Color.FromArgb(10, 12, 26)
			};
			pnlNav.Paint += (s, e) => { //Paint event is used for custom graphics rendering like drawing lines and text on the navigation panel
				using (var pen = new Pen(Color.FromArgb(30, Theme.Blue), 1))
					e.Graphics.DrawLine(pen, pnlNav.Width - 1, 0, pnlNav.Width - 1, pnlNav.Height);
				using (var f = new Font("Consolas", 13, FontStyle.Bold))
				using (var br = new SolidBrush(Theme.Neon))
					e.Graphics.DrawString("◈ HUB", f, br, 26, 18);
			};

			string[] menu = { "SEARCH", "MEMORY", "BALLOON" };
			Color[] mColors = { Theme.Neon, Theme.Purple, Theme.Pink };
			string[] mIcons = { "[ W ]", "[ M ]", "[ B ]" };

			for (int i = 0; i < 3; i++)
			{
				int id = i; Color ac = mColors[i];
				var btn = new Button
				{
					Text = mIcons[i] + "\n" + menu[i],
					Width = 132,
					Height = 82,
					Left = 9,
					Top = 72 + i * 94,
					FlatStyle = FlatStyle.Flat,
					ForeColor = ac,
					BackColor = Color.FromArgb(14, 16, 34),
					Font = new Font("Consolas", 10, FontStyle.Bold),
					Cursor = Cursors.Hand
				};
				btn.FlatAppearance.BorderColor = Color.FromArgb(55, ac);
				btn.FlatAppearance.BorderSize = 1;
				btn.Click += (s, e) => SwitchGame(id);
				btn.MouseEnter += (s, e) => {
					btn.BackColor = Color.FromArgb(24, ac.R, ac.G, ac.B);
					btn.FlatAppearance.BorderColor = ac;
				};
				btn.MouseLeave += (s, e) => {
					btn.BackColor = Color.FromArgb(14, 16, 34);
					btn.FlatAppearance.BorderColor = Color.FromArgb(55, ac);
				};
				pnlNav.Controls.Add(btn);
			}

			var btnClear = new Button
			{
				Text = "✕ CLEAR",
				Dock = DockStyle.Bottom,
				Height = 52,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(38, 12, 8),
				ForeColor = Color.FromArgb(220, 70, 50),
				Font = new Font("Consolas", 10, FontStyle.Bold)
			};
			btnClear.FlatAppearance.BorderColor = Color.FromArgb(70, 200, 50, 30);
			btnClear.Click += (s, e) => ClearSelection();
			pnlNav.Controls.Add(btnClear);
			this.Controls.Add(pnlNav);

			pnlWordBank = new Panel
			{
				Dock = DockStyle.Right,
				Width = 178,
				BackColor = Color.FromArgb(10, 12, 26)
			};
			pnlWordBank.Paint += (s, e) => {
				using (var pen = new Pen(Color.FromArgb(30, Theme.Blue), 1))
					e.Graphics.DrawLine(pen, 0, 0, 0, pnlWordBank.Height);
				using (var f = new Font("Consolas", 10, FontStyle.Bold))
				using (var br = new SolidBrush(Theme.TextDim))
					e.Graphics.DrawString("FIND THESE:", f, br, 12, 14);
				using (var pen = new Pen(Color.FromArgb(30, Theme.Neon), 1))
					e.Graphics.DrawLine(pen, 10, 38, 158, 38);
			};
			this.Controls.Add(pnlWordBank);

			pnlGame.Dock = DockStyle.Fill;
			pnlGame.BackColor = Theme.BgPanel;
			this.Controls.Add(pnlGame);
		}

		private void RefreshStats()
		{
			lblStats.Text = string.Format(
				"  ◈ TIME: {0:D2}:{1:D2}   ◈ SCORE: {2:D5}   ◈ INPUT: {3}",
				totalSeconds / 60, totalSeconds % 60, score, currentInput);
			lblStats.ForeColor = totalSeconds < 30 ? Theme.Pink : Theme.Neon;

			if (wordSearchDone && memoryDone && balloonsDone)
			{
				globalClock.Stop();
				MessageBox.Show("MISSION ACCOMPLISHED! ALL SYSTEMS CLEAR.", "Victory");
				this.Close();
			}
		}

		private void RebuildWordBank()
		{
			foreach (var l in wbLabels) pnlWordBank.Controls.Remove(l);
			wbLabels.Clear();
			var all = new List<string> { "SPACE", "ATOM", "STARS", "MOON", "CELL" };
			for (int i = 0; i < all.Count; i++)
			{
				bool found = !targets.Contains(all[i]);
				var lbl = new Label
				{
					Text = (found ? "✔ " : "» ") + all[i],
					Font = new Font("Consolas", 11, FontStyle.Bold),
					ForeColor = found ? Color.FromArgb(60, Theme.Neon) : Theme.Neon,
					BackColor = Color.Transparent,
					AutoSize = false,
					Width = 178,
					Height = 32,
					Top = 46 + i * 38,
					TextAlign = ContentAlignment.MiddleLeft,
					Padding = new Padding(10, 0, 0, 0)
				};
				pnlWordBank.Controls.Add(lbl);
				wbLabels.Add(lbl);
			}
		}

		private void SwitchGame(int id)
		{
			pnlGame.Controls.Clear();
			ClearSelection();
			pnlWordBank.Visible = (id == 0);
			if (id == 0) LoadWordSearch();
			else if (id == 1) LoadMemory();
			else LoadBalloons();
		}

		private void ClearSelection()
		{
			foreach (var b in selectedButtons)
				if (b.Enabled) b.BackColor = Color.FromArgb(18, 22, 46);
			selectedButtons.Clear();
			currentInput = "";
			lastSelRow = -1;
			lastSelCol = -1;
			RefreshStats();
		}

		private class WordEntry
		{
			public string Word;
			public List<(int r, int c)> Cells = new List<(int, int)>();
			public bool Found;
		}

		// ─────────────────────────────────────────────────────────────────────
		//  WORD SEARCH
		//  FIX: Use a SplitContainer so the header panel has a fixed pixel height
		//       and the grid panel gets exactly the remaining height — no clipping.
		// ─────────────────────────────────────────────────────────────────────
		private void LoadWordSearch()
		{
			if (wordSearchDone) { ShowDone("WORD SEARCH", "All words found!", Theme.Neon); return; }

			var allWords = new List<string> { "SPACE", "ATOM", "STARS", "MOON", "CELL" };
			if (wordEntries.Count == 0)
				foreach (var w in allWords)
					wordEntries.Add(new WordEntry { Word = w, Found = false });

			RebuildWordBank();

			char[,] map = new char[10, 10];
			var rnd = new Random();
			(int dr, int dc)[] dirs = { (0, 1), (1, 0) };

			foreach (var entry in wordEntries)
			{
				if (entry.Found) continue;
				string word = entry.Word;
				int L = word.Length;
				bool placed = false;
				var shuffledDirs = dirs.OrderBy(_ => rnd.Next()).ToArray();

				for (int attempt = 0; attempt < 800 && !placed; attempt++)
				{
					var (dr, dc) = shuffledDirs[attempt % shuffledDirs.Length];
					int maxStartR = 9 - dr * (L - 1);
					int maxStartC = 9 - dc * (L - 1);
					if (maxStartR < 0 || maxStartC < 0) continue;
					int startR = rnd.Next(0, maxStartR + 1);
					int startC = rnd.Next(0, maxStartC + 1);
					bool ok = true;
					for (int i = 0; i < L && ok; i++)
					{
						char ex = map[startR + dr * i, startC + dc * i];
						if (ex != '\0' && ex != word[i]) ok = false;
					}
					if (!ok) continue;
					entry.Cells.Clear();
					for (int i = 0; i < L; i++)
					{
						int cr = startR + dr * i, cc = startC + dc * i;
						map[cr, cc] = word[i];
						entry.Cells.Add((cr, cc));
					}
					placed = true;
				}

				if (!placed)
				{
					for (int row = 0; row < 10 && !placed; row++)
					{
						for (int col = 0; col <= 10 - L && !placed; col++)
						{
							bool ok = true;
							for (int i = 0; i < L && ok; i++)
							{
								char ex = map[row, col + i];
								if (ex != '\0' && ex != word[i]) ok = false;
							}
							if (ok)
							{
								entry.Cells.Clear();
								for (int i = 0; i < L; i++)
								{
									map[row, col + i] = word[i];
									entry.Cells.Add((row, col + i));
								}
								placed = true;
							}
						}
					}
				}
				if (!placed)
					System.Diagnostics.Debug.WriteLine("WARN: could not place word: " + word);
			}

			for (int r = 0; r < 10; r++)
				for (int c = 0; c < 10; c++)
					if (map[r, c] == '\0') map[r, c] = (char)rnd.Next(65, 91);

			// ── ROOT: outer wrapper fills the game panel ──────────────────────
			var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BgPanel };

			// ── HEADER: fixed-height panel docked to Top ──────────────────────
			// Adding Dock=Top controls to wrapper BEFORE the Fill control means
			// WinForms reserves their height first, leaving the rest for the grid.
			var header = new Panel
			{
				Dock = DockStyle.Top,
				Height = 74,          // title 46 + hint 28 — exact, no rounding
				BackColor = Color.Transparent
			};
			header.Controls.Add(new Label
			{
				Text = "◈  W O R D   S E A R C H  ◈",
				Font = new Font("Consolas", 15, FontStyle.Bold),
				ForeColor = Theme.Neon,
				BackColor = Color.Transparent,
				Dock = DockStyle.Top,
				Height = 46,
				TextAlign = ContentAlignment.MiddleCenter
			});
			header.Controls.Add(new Label
			{
				Text = "Click letters in order — they must be adjacent in a straight line.",
				Font = new Font("Consolas", 9),
				ForeColor = Theme.TextDim,
				BackColor = Color.Transparent,
				Dock = DockStyle.Top,
				Height = 28,
				TextAlign = ContentAlignment.MiddleCenter
			});
			wrapper.Controls.Add(header);   // add Top docked panels BEFORE Fill

			// ── GRID OUTER: fills all remaining space ─────────────────────────
			// A 3×3 TableLayoutPanel keeps the inner square centred.
			var gridOuter = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 3,
				RowCount = 3,
				BackColor = Color.Transparent
			};
			gridOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			gridOuter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			gridOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			gridOuter.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
			gridOuter.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			gridOuter.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

			var gridContainer = new Panel
			{
				BackColor = Color.FromArgb(12, 15, 32),
				Margin = new Padding(12)
			};
			gridContainer.Paint += (s, e) =>
				Theme.DrawGlowBorder(e.Graphics,
					new Rectangle(2, 2, gridContainer.Width - 5, gridContainer.Height - 5),
					Theme.Neon, 6);

			// Keep container square — uses the OUTER panel's size, not the form's.
			// The outer panel size already accounts for the header height.
			EventHandler squarify = (s, e) => {
				if (gridOuter.ClientSize.Width < 10 || gridOuter.ClientSize.Height < 10) return;
				int side = Math.Min(gridOuter.ClientSize.Width - 24,
									gridOuter.ClientSize.Height - 24);
				side = Math.Max(side, 200);
				gridContainer.Width = side;
				gridContainer.Height = side;
			};
			gridOuter.Resize += squarify;
			gridOuter.Layout += (s, e) => squarify(s, EventArgs.Empty);

			var grid = new TableLayoutPanel
			{
				ColumnCount = 10,
				RowCount = 10,
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent,
				Padding = new Padding(8),
				CellBorderStyle = TableLayoutPanelCellBorderStyle.None
			};
			for (int i = 0; i < 10; i++)
			{
				grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
				grid.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));
			}

			gridButtons = new Button[10, 10];

			for (int r = 0; r < 10; r++)
			{
				for (int c = 0; c < 10; c++)
				{
					int br = r, bc = c;
					bool preFound = wordEntries.Any(we =>
						we.Found && we.Cells.Any(cell => cell.r == br && cell.c == bc));

					var btn = new Button
					{
						Text = map[r, c].ToString(),
						Dock = DockStyle.Fill,
						FlatStyle = FlatStyle.Flat,
						BackColor = preFound ? Color.FromArgb(20, 80, 50) : Color.FromArgb(18, 22, 46),
						ForeColor = preFound ? Theme.Neon : Theme.TextBright,
						Margin = new Padding(2),
						Font = new Font("Consolas", 13, FontStyle.Bold),
						Cursor = Cursors.Hand,
						Enabled = !preFound
					};
					btn.FlatAppearance.BorderColor = Color.FromArgb(32, Theme.Blue);
					btn.FlatAppearance.BorderSize = 1;
					btn.MouseEnter += (s, e) => {
						if (btn.Enabled && !selectedButtons.Contains(btn))
							btn.BackColor = Color.FromArgb(30, 38, 70);
					};
					btn.MouseLeave += (s, e) => {
						if (btn.Enabled && !selectedButtons.Contains(btn))
							btn.BackColor = Color.FromArgb(18, 22, 46);
					};
					btn.Click += (s, e) => OnWordCellClick(btn, br, bc);

					gridButtons[r, c] = btn;
					grid.Controls.Add(btn, c, r);
				}
			}

			gridContainer.Controls.Add(grid);
			gridOuter.Controls.Add(gridContainer, 1, 1);
			wrapper.Controls.Add(gridOuter);   // Fill — added AFTER Top panels
			pnlGame.Controls.Add(wrapper);
		}

		private void OnWordCellClick(Button b, int row, int col)
		{
			if (!b.Enabled || selectedButtons.Contains(b)) return;

			if (selectedButtons.Count > 0)
			{
				int dr = row - lastSelRow, dc = col - lastSelCol;
				if (Math.Abs(dr) > 1 || Math.Abs(dc) > 1 || (dr == 0 && dc == 0))
				{ ClearSelection(); }
				else if (selectedButtons.Count >= 2)
				{
					int prevDr = lastSelRow - GetGridRow(selectedButtons[selectedButtons.Count - 2]);
					int prevDc = lastSelCol - GetGridCol(selectedButtons[selectedButtons.Count - 2]);
					if (dr != prevDr || dc != prevDc) ClearSelection();
				}
			}

			b.BackColor = Color.FromArgb(180, 100, 0);
			b.ForeColor = Color.White;
			selectedButtons.Add(b);
			currentInput += b.Text;
			lastSelRow = row;
			lastSelCol = col;

			var found = wordEntries.FirstOrDefault(we =>
				!we.Found && we.Word == currentInput && SelectionMatchesEntry(we));

			if (found != null)
			{
				score += 100;
				found.Found = true;
				targets.Remove(found.Word);
				foreach (var sb in selectedButtons)
				{
					sb.BackColor = Color.FromArgb(20, 80, 50);
					sb.ForeColor = Theme.Neon;
					sb.Enabled = false;
				}
				ClearSelection();
				RebuildWordBank();
				if (targets.Count == 0)
				{
					wordSearchDone = true;
					var t = new Timer { Interval = 700 };
					t.Tick += (st, se) => { t.Stop(); SwitchGame(0); };
					t.Start();
				}
			}
			else if (currentInput.Length > 10) ClearSelection();

			RefreshStats();
		}

		private bool SelectionMatchesEntry(WordEntry entry)
		{
			if (selectedButtons.Count != entry.Cells.Count) return false;
			for (int i = 0; i < selectedButtons.Count; i++)
			{
				var (er, ec) = entry.Cells[i];
				if (GetGridRow(selectedButtons[i]) != er || GetGridCol(selectedButtons[i]) != ec)
					return false;
			}
			return true;
		}

		private int GetGridRow(Button b)
		{
			for (int r = 0; r < 10; r++) for (int c = 0; c < 10; c++) if (gridButtons[r, c] == b) return r;
			return -1;
		}
		private int GetGridCol(Button b)
		{
			for (int r = 0; r < 10; r++) for (int c = 0; c < 10; c++) if (gridButtons[r, c] == b) return c;
			return -1;
		}

		// ─────────────────────────────────────────────────────────────────────
		//  MEMORY MATCH
		//  FIX: In WinForms, a Dock=Fill control must be added to Controls
		//       BEFORE any Dock=Top/Bottom controls. The layout engine processes
		//       edges first; if Fill is added last it gets clipped by whatever
		//       space remains after the engine already positioned edge controls.
		//       Solution: add memGrid first, then add the Top-docked labels.
		// ─────────────────────────────────────────────────────────────────────
		private void LoadMemory()
		{
			pnlGame.Controls.Clear();
			if (memoryDone) { ShowDone("MEMORY MATCH", "All pairs matched!", Theme.Purple); return; }

			var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BgPanel };

			// ── Build the 4×4 grid first (Dock=Fill) ─────────────────────────
			// IMPORTANT: add this to wrapper BEFORE the Dock=Top labels below.
			var memGrid = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 4,
				RowCount = 4,
				Padding = new Padding(16, 8, 16, 16),   // less top padding; header gives top space
				BackColor = Color.Transparent,
				CellBorderStyle = TableLayoutPanelCellBorderStyle.None
			};
			for (int i = 0; i < 4; i++)
			{
				memGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
				memGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			}

			string[] words = { "ATOM", "DNA", "BOND", "CELL", "STAR", "IRON", "O2", "GOLD" };
			string[] syms = { "⚛", "DNA", "⚗", "◉", "★", "Fe", "O²", "AU" };
			Color[] accents = {
				Theme.Neon,  Theme.Blue,              Theme.Purple,            Theme.Pink,
				Theme.Gold,  Color.FromArgb(100,200,255), Color.FromArgb(255,160,60), Color.FromArgb(60,220,180)
			};
			var accentMap = new Dictionary<string, Color>();
			var symMap = new Dictionary<string, string>();
			for (int i = 0; i < words.Length; i++) { accentMap[words[i]] = accents[i]; symMap[words[i]] = syms[i]; }

			var gameSet = new List<string>(words);
			gameSet.AddRange(words);
			var shuffled = gameSet.OrderBy(_ => Guid.NewGuid()).ToList();

			// lblPairs reference needed inside card click handler
			var lblPairs = new Label
			{
				Text = "PAIRS FOUND:  0 / 8",
				Font = new Font("Consolas", 11),
				ForeColor = Theme.TextDim,
				BackColor = Color.Transparent,
				Dock = DockStyle.Top,
				Height = 28,
				TextAlign = ContentAlignment.MiddleCenter
			};

			for (int i = 0; i < 16; i++)
			{
				string word = shuffled[i];
				Color accent = accentMap[word];
				string sym = symMap[word];

				var card = new Button
				{
					Dock = DockStyle.Fill,
					FlatStyle = FlatStyle.Flat,
					BackColor = Color.FromArgb(18, 22, 46),
					ForeColor = Theme.Blue,
					Font = new Font("Consolas", 11, FontStyle.Bold),
					Tag = word,
					Text = "?",
					Margin = new Padding(7),
					Cursor = Cursors.Hand
				};
				card.FlatAppearance.BorderColor = Color.FromArgb(45, Theme.Blue);
				card.FlatAppearance.BorderSize = 1;

				card.MouseEnter += (s, e) => {
					if (card.Text == "?" && card.Enabled)
						card.BackColor = Color.FromArgb(28, 32, 62);
				};
				card.MouseLeave += (s, e) => {
					if (card.Text == "?" && card.Enabled)
						card.BackColor = Color.FromArgb(18, 22, 46);
				};

				card.Click += (s, e) => {
					if (card.Text != "?" || !card.Enabled) return;

					card.Text = sym + "  " + word;
					card.BackColor = Color.FromArgb(28, accent.R, accent.G, accent.B);
					card.ForeColor = accent;
					card.FlatAppearance.BorderColor = accent;

					if (memFirst == null)
					{
						memFirst = card;
					}
					else
					{
						pnlGame.Enabled = false;
						if (memFirst.Tag.ToString() == card.Tag.ToString() && memFirst != card)
						{
							score += 50; memoryMatches++;
							card.Enabled = memFirst.Enabled = false;
							card.BackColor = memFirst.BackColor = Color.FromArgb(20, 80, 50);
							card.ForeColor = memFirst.ForeColor = Theme.Neon;
							card.FlatAppearance.BorderColor = Theme.Neon;
							memFirst.FlatAppearance.BorderColor = Theme.Neon;
							memFirst = null;
							pnlGame.Enabled = true;
							lblPairs.Text = string.Format("PAIRS FOUND:  {0} / 8", memoryMatches);
							if (memoryMatches == 8)
							{
								memoryDone = true;
								RefreshStats();
								var t = new Timer { Interval = 700 };
								t.Tick += (st, se) => { t.Stop(); SwitchGame(1); };
								t.Start();
							}
						}
						else
						{
							var firstRef = memFirst; memFirst = null;
							var flip = new Timer { Interval = 800 };
							flip.Tick += (st, se) => {
								flip.Stop();
								foreach (var cr in new[] { card, firstRef })
								{
									cr.Text = "?";
									cr.BackColor = Color.FromArgb(18, 22, 46);
									cr.ForeColor = Theme.Blue;
									cr.FlatAppearance.BorderColor = Color.FromArgb(45, Theme.Blue);
								}
								pnlGame.Enabled = true;
							};
							flip.Start();
						}
					}
				};
				memGrid.Controls.Add(card);
			}

			// ── Add Fill control FIRST, then Top-docked controls ─────────────
			// This is the critical WinForms rule: Fill must precede edge-docked
			// siblings so the engine gives it whatever space is left over after
			// the edge panels are measured.
			wrapper.Controls.Add(memGrid);   // Fill  ← FIRST

			wrapper.Controls.Add(lblPairs);  // Top   ← AFTER Fill
			wrapper.Controls.Add(new Label
			{ // Top   ← AFTER Fill
				Text = "◈  M E M O R Y   M A T C H  ◈",
				Font = new Font("Consolas", 15, FontStyle.Bold),
				ForeColor = Theme.Purple,
				BackColor = Color.Transparent,
				Dock = DockStyle.Top,
				Height = 48,
				TextAlign = ContentAlignment.MiddleCenter
			});

			pnlGame.Controls.Add(wrapper);
		}

		// ─────────────────────────────────────────────────────────────────────
		//  BALLOON POP
		// ─────────────────────────────────────────────────────────────────────
		private void LoadBalloons()
		{
			pnlGame.Controls.Clear();
			if (balloonsDone) { ShowDone("BALLOON POP", "All balloons popped!", Theme.Pink); return; }

			var area = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(6, 8, 22) };

			area.Paint += (s, e) => {
				var g = e.Graphics;
				var rr = new Random(77);
				for (int i = 0; i < 70; i++)
				{
					int x = rr.Next(area.Width), y = rr.Next(area.Height - 70);
					using (var br = new SolidBrush(Color.FromArgb(rr.Next(30, 110), 180, 210, 255)))
						g.FillEllipse(br, x, y, rr.Next(1, 3), rr.Next(1, 3));
				}
			};

			area.Controls.Add(new Label
			{
				Text = "◈  B A L L O O N   P O P  ◈",
				Font = new Font("Consolas", 15, FontStyle.Bold),
				ForeColor = Theme.Pink,
				BackColor = Color.Transparent,
				Dock = DockStyle.Top,
				Height = 48,
				TextAlign = ContentAlignment.MiddleCenter
			});

			var bottomBar = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 70,
				BackColor = Color.FromArgb(10, 12, 28)
			};
			var lblC = new Label
			{
				Text = string.Format("  ◈ POPPED: {0} / 15", balloonsPopped),
				Font = new Font("Consolas", 13, FontStyle.Bold),
				ForeColor = Theme.Pink,
				BackColor = Color.Transparent,
				AutoSize = false,
				Width = 400,
				Height = 30,
				Top = 6,
				TextAlign = ContentAlignment.MiddleLeft
			};
			bottomBar.Controls.Add(lblC);
			bottomBar.Paint += (s, e) => {
				var g = e.Graphics;
				g.SmoothingMode = SmoothingMode.AntiAlias;
				using (var pen = new Pen(Color.FromArgb(40, Theme.Pink), 1))
					g.DrawLine(pen, 0, 0, bottomBar.Width, 0);
				int barW = bottomBar.Width - 24;
				int fillW = (int)(balloonsPopped / 15.0 * barW);
				using (var br = new SolidBrush(Color.FromArgb(28, 32, 55)))
					g.FillRectangle(br, 12, 38, barW, 16);
				if (fillW > 0)
					using (var lgb = new LinearGradientBrush(
						new Point(12, 38), new Point(12 + fillW, 38),
						Theme.Pink, Theme.Purple))
						g.FillRectangle(lgb, 12, 38, fillW, 16);
				using (var pen = new Pen(Color.FromArgb(70, Theme.Pink), 1))
					g.DrawRectangle(pen, 12, 38, barW, 16);
			};
			area.Controls.Add(bottomBar);

			Color[] bColors = {
				Color.FromArgb(255,55,130), Color.FromArgb(100,80,255),
				Color.FromArgb(0,200,255),  Color.FromArgb(255,180,0),
				Color.FromArgb(60,220,120)
			};
			var rnd = new Random();

			for (int i = 0; i < 10; i++)
			{
				int bw = rnd.Next(44, 68);
				int bh = (int)(bw * 1.35);
				int speed = rnd.Next(3, 9);
				Color col = bColors[rnd.Next(bColors.Length)];
				int bww = bw, bhh = bh;
				Color cc = col;

				var b = new Button
				{
					Size = new Size(bw, bh + 18),
					FlatStyle = FlatStyle.Flat,
					BackColor = Color.Transparent,
					Cursor = Cursors.Hand
				};
				b.FlatAppearance.BorderSize = 0;
				b.FlatAppearance.MouseOverBackColor = Color.Transparent;
				b.FlatAppearance.MouseDownBackColor = Color.Transparent;
				b.Location = new Point(rnd.Next(30, 720), rnd.Next(520, 920));

				b.Paint += (s, e) => {
					var g = e.Graphics;
					g.SmoothingMode = SmoothingMode.AntiAlias;
					using (var br = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
						g.FillEllipse(br, 5, 5, bww, bhh);
					using (var lgb = new LinearGradientBrush(
						new Point(2, 2), new Point(bww - 2, bhh - 2),
						Color.FromArgb(230, cc),
						Color.FromArgb(180, Math.Max(0, cc.R - 90), Math.Max(0, cc.G - 90), Math.Max(0, cc.B - 90))))
						g.FillEllipse(lgb, 2, 2, bww - 2, bhh - 2);
					using (var br = new SolidBrush(Color.FromArgb(75, 255, 255, 255)))
						g.FillEllipse(br, bww / 4, bhh / 8, bww / 3, bhh / 4);
					using (var br = new SolidBrush(Color.FromArgb(200,
							Math.Max(0, cc.R - 70), Math.Max(0, cc.G - 70), Math.Max(0, cc.B - 70))))
						g.FillPolygon(br, new Point[] {
							new Point(bww/2+1, bhh+2),
							new Point(bww/2-5, bhh+14),
							new Point(bww/2+7, bhh+14) });
					using (var pen = new Pen(Color.FromArgb(100, cc), 1))
						g.DrawLine(pen, bww / 2 + 1, bhh + 14, bww / 2 + 1, bhh + 18);
				};

				var t = new Timer { Interval = 22 };
				t.Tick += (s, e) => { b.Top -= speed; if (b.Bottom < 50) b.Top = area.Height - 80; };

				b.MouseDown += (s, e) => {
					score += 20; balloonsPopped++;
					lblC.Text = string.Format("  ◈ POPPED: {0} / 15", balloonsPopped);
					bottomBar.Invalidate();
					b.Top = area.Height + 200;
					if (balloonsPopped >= 15) { t.Stop(); balloonsDone = true; RefreshStats(); SwitchGame(2); }
				};

				area.Controls.Add(b);
				t.Start();
			}

			pnlGame.Controls.Add(area);
		}

		private void ShowDone(string gameName, string subMsg, Color accent)
		{
			pnlGame.Controls.Clear();
			var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BgPanel };

			panel.Paint += (s, e) => {
				var g = e.Graphics;
				g.SmoothingMode = SmoothingMode.AntiAlias;
				int cx = panel.Width / 2, cy = panel.Height / 2 - 40;
				for (int r = 130; r >= 20; r -= 10)
					using (var br = new SolidBrush(Color.FromArgb(5, accent.R, accent.G, accent.B)))
						g.FillEllipse(br, cx - r, cy - r, r * 2, r * 2);
			};

			panel.Controls.Add(new Label
			{
				Text = "✔",
				Font = new Font("Segoe UI Symbol", 64),
				ForeColor = accent,
				BackColor = Color.Transparent,
				Dock = DockStyle.None,
				AutoSize = false,
				Width = 700,
				Height = 110,
				Top = 130,
				Left = (pnlGame.Width - 700) / 2,
				TextAlign = ContentAlignment.MiddleCenter
			});
			panel.Controls.Add(new Label
			{
				Text = gameName,
				Font = new Font("Consolas", 22, FontStyle.Bold),
				ForeColor = accent,
				BackColor = Color.Transparent,
				AutoSize = false,
				Width = 700,
				Height = 55,
				Top = 248,
				Left = (pnlGame.Width - 700) / 2,
				TextAlign = ContentAlignment.MiddleCenter
			});
			panel.Controls.Add(new Label
			{
				Text = subMsg + "\n\nSCORE: " + score,
				Font = new Font("Consolas", 13),
				ForeColor = Theme.TextDim,
				BackColor = Color.Transparent,
				AutoSize = false,
				Width = 700,
				Height = 80,
				Top = 314,
				Left = (pnlGame.Width - 700) / 2,
				TextAlign = ContentAlignment.MiddleCenter
			});

			pnlGame.Controls.Add(panel);
		}
	}
}