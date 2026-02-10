/* ========================================
   贪吃蛇游戏核心逻辑
   ======================================== */

class SnakeGame {
    constructor(canvas, callbacks = {}) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.callbacks = callbacks;

        // 游戏配置
        this.gridSize = 20;
        this.tileCount = canvas.width / this.gridSize;

        // 游戏状态
        this.isRunning = false;
        this.isPaused = false;
        this.score = 0;
        this.gameLoop = null;
        this.speed = 100; // 毫秒

        // 蛇
        this.snake = [];
        this.direction = { x: 1, y: 0 };
        this.nextDirection = { x: 1, y: 0 };

        // 食物
        this.food = { x: 0, y: 0 };

        // 颜色配置
        this.colors = {
            background: '#0f172a',
            snake: {
                head: '#00d4ff',
                body: '#00a8cc',
                glow: 'rgba(0, 212, 255, 0.3)'
            },
            food: {
                main: '#d946ef',
                glow: 'rgba(217, 70, 239, 0.4)'
            },
            grid: 'rgba(255, 255, 255, 0.03)'
        };

        // 绑定键盘事件
        this.handleKeyDown = this.handleKeyDown.bind(this);
        document.addEventListener('keydown', this.handleKeyDown);

        // 初始速度设置
        this.baseSpeed = 100;
        this.speed = this.baseSpeed;

        // 初始绘制
        this.draw();
    }

    /**
     * 设置游戏速度
     * @param {number} ms - 每帧毫秒数（越小越快）
     */
    setSpeed(ms) {
        this.baseSpeed = ms;
        this.speed = ms;

        // 如果游戏正在运行且未暂停，更新游戏循环
        if (this.isRunning && !this.isPaused) {
            clearInterval(this.gameLoop);
            this.gameLoop = setInterval(() => this.update(), this.speed);
        }
    }

    /**
     * 初始化游戏
     */
    init() {
        // 初始化蛇（从中间开始）
        const startX = Math.floor(this.tileCount / 2);
        const startY = Math.floor(this.tileCount / 2);
        this.snake = [
            { x: startX, y: startY },
            { x: startX - 1, y: startY },
            { x: startX - 2, y: startY }
        ];

        // 初始化方向
        this.direction = { x: 1, y: 0 };
        this.nextDirection = { x: 1, y: 0 };

        // 初始化分数
        this.score = 0;
        if (this.callbacks.onScoreChange) {
            this.callbacks.onScoreChange(this.score);
        }

        // 生成食物
        this.generateFood();
    }

    /**
     * 开始游戏
     */
    start() {
        if (this.isRunning) return;

        this.init();
        this.isRunning = true;
        this.isPaused = false;

        if (this.callbacks.onGameStart) {
            this.callbacks.onGameStart();
        }

        this.gameLoop = setInterval(() => this.update(), this.speed);
    }

    /**
     * 暂停游戏
     */
    pause() {
        if (!this.isRunning) return;

        this.isPaused = !this.isPaused;

        if (this.isPaused) {
            clearInterval(this.gameLoop);
        } else {
            this.gameLoop = setInterval(() => this.update(), this.speed);
        }
    }

    /**
     * 结束游戏
     */
    gameOver() {
        this.isRunning = false;
        clearInterval(this.gameLoop);

        if (this.callbacks.onGameOver) {
            this.callbacks.onGameOver(this.score);
        }
    }

    /**
     * 更新游戏状态
     */
    update() {
        // 更新方向
        this.direction = { ...this.nextDirection };

        // 计算新头部位置
        const head = this.snake[0];
        const newHead = {
            x: head.x + this.direction.x,
            y: head.y + this.direction.y
        };

        // 检查碰撞
        if (this.checkCollision(newHead)) {
            this.gameOver();
            return;
        }

        // 移动蛇
        this.snake.unshift(newHead);

        // 检查是否吃到食物
        if (newHead.x === this.food.x && newHead.y === this.food.y) {
            this.score += 10;
            if (this.callbacks.onScoreChange) {
                this.callbacks.onScoreChange(this.score);
            }
            this.generateFood();

            // 加速（每吃5个食物加速一次，最快50ms）
            if (this.score % 50 === 0 && this.speed > 50) {
                this.speed -= 5;
                clearInterval(this.gameLoop);
                this.gameLoop = setInterval(() => this.update(), this.speed);
            }
        } else {
            // 没吃到食物，移除尾部
            this.snake.pop();
        }

        // 重新绘制
        this.draw();
    }

    /**
     * 检查碰撞
     */
    checkCollision(head) {
        // 边界碰撞
        if (head.x < 0 || head.x >= this.tileCount ||
            head.y < 0 || head.y >= this.tileCount) {
            return true;
        }

        // 自身碰撞
        for (let i = 0; i < this.snake.length; i++) {
            if (head.x === this.snake[i].x && head.y === this.snake[i].y) {
                return true;
            }
        }

        return false;
    }

    /**
     * 生成食物
     */
    generateFood() {
        let newFood;
        let attempts = 0;
        const maxAttempts = 100;

        do {
            newFood = {
                x: Math.floor(Math.random() * this.tileCount),
                y: Math.floor(Math.random() * this.tileCount)
            };
            attempts++;
        } while (this.isOnSnake(newFood) && attempts < maxAttempts);

        this.food = newFood;
    }

    /**
     * 检查位置是否在蛇身上
     */
    isOnSnake(pos) {
        return this.snake.some(segment =>
            segment.x === pos.x && segment.y === pos.y
        );
    }

    /**
     * 处理键盘输入
     */
    handleKeyDown(e) {
        if (!this.isRunning) return;

        const key = e.key.toLowerCase();

        // 暂停
        if (e.code === 'Escape' || key === 'p') {
            this.pause();
            return;
        }

        // 方向控制
        const directions = {
            'arrowup': { x: 0, y: -1 },
            'arrowdown': { x: 0, y: 1 },
            'arrowleft': { x: -1, y: 0 },
            'arrowright': { x: 1, y: 0 },
            'w': { x: 0, y: -1 },
            's': { x: 0, y: 1 },
            'a': { x: -1, y: 0 },
            'd': { x: 1, y: 0 }
        };

        const newDir = directions[key] || directions[e.key];

        if (newDir) {
            // 防止180度转向
            if (newDir.x !== -this.direction.x || newDir.y !== -this.direction.y) {
                this.nextDirection = newDir;
            }
            e.preventDefault();
        }
    }

    /**
     * 绘制游戏
     */
    draw() {
        const ctx = this.ctx;
        const gs = this.gridSize;

        // 清空画布
        ctx.fillStyle = this.colors.background;
        ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        // 绘制网格
        this.drawGrid();

        // 绘制食物
        this.drawFood();

        // 绘制蛇
        this.drawSnake();

        // 如果暂停，显示暂停文字
        if (this.isPaused) {
            this.drawPauseOverlay();
        }
    }

    /**
     * 绘制网格
     */
    drawGrid() {
        const ctx = this.ctx;
        const gs = this.gridSize;

        ctx.strokeStyle = this.colors.grid;
        ctx.lineWidth = 1;

        for (let i = 0; i <= this.tileCount; i++) {
            // 垂直线
            ctx.beginPath();
            ctx.moveTo(i * gs, 0);
            ctx.lineTo(i * gs, this.canvas.height);
            ctx.stroke();

            // 水平线
            ctx.beginPath();
            ctx.moveTo(0, i * gs);
            ctx.lineTo(this.canvas.width, i * gs);
            ctx.stroke();
        }
    }

    /**
     * 绘制蛇
     */
    drawSnake() {
        const ctx = this.ctx;
        const gs = this.gridSize;

        this.snake.forEach((segment, index) => {
            const x = segment.x * gs;
            const y = segment.y * gs;

            // 发光效果
            const glow = index === 0 ? 15 : 8;
            ctx.shadowColor = this.colors.snake.glow;
            ctx.shadowBlur = glow;

            // 颜色渐变（头到尾）
            const ratio = index / this.snake.length;
            const color = index === 0 ? this.colors.snake.head : this.colors.snake.body;
            const alpha = 1 - ratio * 0.5;

            ctx.fillStyle = color;
            ctx.globalAlpha = alpha;

            // 圆角矩形
            const padding = 2;
            const radius = 4;
            this.roundRect(ctx, x + padding, y + padding, gs - padding * 2, gs - padding * 2, radius);
            ctx.fill();

            // 头部眼睛
            if (index === 0) {
                this.drawEyes(segment);
            }

            ctx.globalAlpha = 1;
            ctx.shadowBlur = 0;
        });
    }

    /**
     * 绘制蛇眼睛
     */
    drawEyes(head) {
        const ctx = this.ctx;
        const gs = this.gridSize;
        const x = head.x * gs;
        const y = head.y * gs;

        ctx.fillStyle = '#0f172a';
        ctx.shadowBlur = 0;

        const eyeSize = 3;
        const eyeOffset = 5;

        // 根据方向确定眼睛位置
        let eye1, eye2;
        if (this.direction.x === 1) { // 右
            eye1 = { x: x + gs - 7, y: y + 5 };
            eye2 = { x: x + gs - 7, y: y + gs - 8 };
        } else if (this.direction.x === -1) { // 左
            eye1 = { x: x + 4, y: y + 5 };
            eye2 = { x: x + 4, y: y + gs - 8 };
        } else if (this.direction.y === -1) { // 上
            eye1 = { x: x + 5, y: y + 4 };
            eye2 = { x: x + gs - 8, y: y + 4 };
        } else { // 下
            eye1 = { x: x + 5, y: y + gs - 7 };
            eye2 = { x: x + gs - 8, y: y + gs - 7 };
        }

        ctx.beginPath();
        ctx.arc(eye1.x, eye1.y, eyeSize, 0, Math.PI * 2);
        ctx.fill();

        ctx.beginPath();
        ctx.arc(eye2.x, eye2.y, eyeSize, 0, Math.PI * 2);
        ctx.fill();
    }

    /**
     * 绘制食物
     */
    drawFood() {
        const ctx = this.ctx;
        const gs = this.gridSize;
        const x = this.food.x * gs;
        const y = this.food.y * gs;

        // 发光效果
        ctx.shadowColor = this.colors.food.glow;
        ctx.shadowBlur = 15;

        // 绘制食物（圆形）
        const centerX = x + gs / 2;
        const centerY = y + gs / 2;
        const radius = gs / 2 - 3;

        // 渐变填充
        const gradient = ctx.createRadialGradient(
            centerX - 2, centerY - 2, 0,
            centerX, centerY, radius
        );
        gradient.addColorStop(0, '#ff6bef');
        gradient.addColorStop(1, this.colors.food.main);

        ctx.fillStyle = gradient;
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
        ctx.fill();

        ctx.shadowBlur = 0;
    }

    /**
     * 绘制暂停覆盖层
     */
    drawPauseOverlay() {
        const ctx = this.ctx;

        ctx.fillStyle = 'rgba(15, 23, 42, 0.7)';
        ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        ctx.font = 'bold 24px Outfit, sans-serif';
        ctx.fillStyle = '#00d4ff';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('已暂停', this.canvas.width / 2, this.canvas.height / 2);
        ctx.font = '16px Outfit, sans-serif';
        ctx.fillStyle = 'rgba(255, 255, 255, 0.7)';
        ctx.fillText('按 P 或 ESC 继续', this.canvas.width / 2, this.canvas.height / 2 + 30);
    }

    /**
     * 绘制圆角矩形
     */
    roundRect(ctx, x, y, width, height, radius) {
        ctx.beginPath();
        ctx.moveTo(x + radius, y);
        ctx.lineTo(x + width - radius, y);
        ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
        ctx.lineTo(x + width, y + height - radius);
        ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
        ctx.lineTo(x + radius, y + height);
        ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
        ctx.lineTo(x, y + radius);
        ctx.quadraticCurveTo(x, y, x + radius, y);
        ctx.closePath();
    }

    /**
     * 销毁游戏实例
     */
    destroy() {
        clearInterval(this.gameLoop);
        document.removeEventListener('keydown', this.handleKeyDown);
    }
}

// 导出模块（如果是模块化环境）
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SnakeGame;
}
