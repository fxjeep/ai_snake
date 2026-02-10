/* ======================================
   俄罗斯方块 - 核心游戏逻辑
   ====================================== */

(() => {
    'use strict';

    // ===== 常量与配置 =====
    const COLS = 10;
    const ROWS = 20;
    const BLOCK_SIZE = 30; // pixels per cell

    // 游戏状态
    const STATE = { IDLE: 0, PLAYING: 1, PAUSED: 2, GAMEOVER: 3 };

    // 7种标准 Tetromino 的形状矩阵（每种含4个旋转状态）
    const SHAPES = {
        I: [
            [[0,0,0,0],[1,1,1,1],[0,0,0,0],[0,0,0,0]],
            [[0,0,1,0],[0,0,1,0],[0,0,1,0],[0,0,1,0]],
            [[0,0,0,0],[0,0,0,0],[1,1,1,1],[0,0,0,0]],
            [[0,1,0,0],[0,1,0,0],[0,1,0,0],[0,1,0,0]]
        ],
        O: [
            [[1,1],[1,1]],
            [[1,1],[1,1]],
            [[1,1],[1,1]],
            [[1,1],[1,1]]
        ],
        T: [
            [[0,1,0],[1,1,1],[0,0,0]],
            [[0,1,0],[0,1,1],[0,1,0]],
            [[0,0,0],[1,1,1],[0,1,0]],
            [[0,1,0],[1,1,0],[0,1,0]]
        ],
        S: [
            [[0,1,1],[1,1,0],[0,0,0]],
            [[0,1,0],[0,1,1],[0,0,1]],
            [[0,0,0],[0,1,1],[1,1,0]],
            [[1,0,0],[1,1,0],[0,1,0]]
        ],
        Z: [
            [[1,1,0],[0,1,1],[0,0,0]],
            [[0,0,1],[0,1,1],[0,1,0]],
            [[0,0,0],[1,1,0],[0,1,1]],
            [[0,1,0],[1,1,0],[1,0,0]]
        ],
        J: [
            [[1,0,0],[1,1,1],[0,0,0]],
            [[0,1,1],[0,1,0],[0,1,0]],
            [[0,0,0],[1,1,1],[0,0,1]],
            [[0,1,0],[0,1,0],[1,1,0]]
        ],
        L: [
            [[0,0,1],[1,1,1],[0,0,0]],
            [[0,1,0],[0,1,0],[0,1,1]],
            [[0,0,0],[1,1,1],[1,0,0]],
            [[1,1,0],[0,1,0],[0,1,0]]
        ]
    };

    // 方块颜色
    const COLORS = {
        I: '#22d3ee', // cyan
        O: '#fbbf24', // yellow
        T: '#a78bfa', // purple
        S: '#34d399', // green
        Z: '#f87171', // red
        J: '#60a5fa', // blue
        L: '#fb923c'  // orange
    };

    // 方块发光颜色（用于边框/高光）
    const GLOW_COLORS = {
        I: 'rgba(34,211,238,0.5)',
        O: 'rgba(251,191,36,0.5)',
        T: 'rgba(167,139,250,0.5)',
        S: 'rgba(52,211,153,0.5)',
        Z: 'rgba(248,113,113,0.5)',
        J: 'rgba(96,165,250,0.5)',
        L: 'rgba(251,146,60,0.5)'
    };

    // 计分规则
    const SCORE_TABLE = { 1: 100, 2: 300, 3: 500, 4: 800 };

    // 各等级下落速度（ms）
    const LEVEL_SPEEDS = [
        800, 720, 630, 550, 470, 380, 300, 220, 150, 100,
        80, 70, 60, 50, 40, 30, 25, 20, 15, 10
    ];

    const PIECE_TYPES = Object.keys(SHAPES);

    // ===== DOM元素 =====
    const canvas = document.getElementById('gameCanvas');
    const ctx = canvas.getContext('2d');
    const nextCanvas = document.getElementById('nextCanvas');
    const nextCtx = nextCanvas.getContext('2d');
    const canvasContainer = document.getElementById('canvasContainer');
    const gameOverlay = document.getElementById('gameOverlay');
    const overlayTitle = document.getElementById('overlayTitle');
    const overlaySubtitle = document.getElementById('overlaySubtitle');
    const scoreEl = document.getElementById('score');
    const levelEl = document.getElementById('level');
    const linesEl = document.getElementById('lines');
    const btnStart = document.getElementById('btnStart');
    const btnPause = document.getElementById('btnPause');
    const btnRestart = document.getElementById('btnRestart');

    // ===== 游戏状态变量 =====
    let board = [];
    let currentPiece = null;
    let nextPiece = null;
    let score = 0;
    let level = 1;
    let totalLines = 0;
    let gameState = STATE.IDLE;
    let lastDropTime = 0;
    let animationId = null;
    let clearingRows = []; // rows being animated
    let clearAnimStart = 0;
    const CLEAR_ANIM_DURATION = 400; // ms

    // ===== Bag randomizer =====
    let bag = [];

    function refillBag() {
        bag = [...PIECE_TYPES];
        // Fisher-Yates shuffle
        for (let i = bag.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [bag[i], bag[j]] = [bag[j], bag[i]];
        }
    }

    function getNextType() {
        if (bag.length === 0) refillBag();
        return bag.pop();
    }

    // ===== Board =====
    function createBoard() {
        board = [];
        for (let r = 0; r < ROWS; r++) {
            board.push(new Array(COLS).fill(null));
        }
    }

    // ===== Piece =====
    function createPiece(type) {
        const shapes = SHAPES[type];
        return {
            type,
            rotation: 0,
            shape: shapes[0],
            x: Math.floor((COLS - shapes[0][0].length) / 2),
            y: 0
        };
    }

    function spawnPiece() {
        currentPiece = nextPiece || createPiece(getNextType());
        nextPiece = createPiece(getNextType());

        // 检测游戏结束
        if (collides(currentPiece, currentPiece.x, currentPiece.y)) {
            gameOver();
        }
    }

    // ===== 碰撞检测 =====
    function collides(piece, px, py) {
        const shape = piece.shape;
        for (let r = 0; r < shape.length; r++) {
            for (let c = 0; c < shape[r].length; c++) {
                if (!shape[r][c]) continue;
                const newX = px + c;
                const newY = py + r;
                if (newX < 0 || newX >= COLS || newY >= ROWS) return true;
                if (newY >= 0 && board[newY][newX]) return true;
            }
        }
        return false;
    }

    // ===== 方块操作 =====
    function movePiece(dx, dy) {
        if (gameState !== STATE.PLAYING || !currentPiece) return false;
        const nx = currentPiece.x + dx;
        const ny = currentPiece.y + dy;
        if (!collides(currentPiece, nx, ny)) {
            currentPiece.x = nx;
            currentPiece.y = ny;
            return true;
        }
        return false;
    }

    function rotatePiece() {
        if (gameState !== STATE.PLAYING || !currentPiece) return;
        const shapes = SHAPES[currentPiece.type];
        const newRotation = (currentPiece.rotation + 1) % 4;
        const oldShape = currentPiece.shape;
        currentPiece.shape = shapes[newRotation];
        currentPiece.rotation = newRotation;

        // Wall kick: try offsets
        const kicks = [0, -1, 1, -2, 2];
        for (const dx of kicks) {
            if (!collides(currentPiece, currentPiece.x + dx, currentPiece.y)) {
                currentPiece.x += dx;
                return;
            }
        }

        // None worked — revert
        currentPiece.shape = oldShape;
        currentPiece.rotation = (newRotation + 3) % 4;
    }

    function hardDrop() {
        if (gameState !== STATE.PLAYING || !currentPiece) return;
        let dropDistance = 0;
        while (!collides(currentPiece, currentPiece.x, currentPiece.y + 1)) {
            currentPiece.y++;
            dropDistance++;
        }
        score += dropDistance * 2; // bonus for hard drop
        updateScoreDisplay();
        lockPiece();
    }

    function getGhostY() {
        if (!currentPiece) return 0;
        let gy = currentPiece.y;
        while (!collides(currentPiece, currentPiece.x, gy + 1)) {
            gy++;
        }
        return gy;
    }

    // ===== 锁定与行消除 =====
    function lockPiece() {
        const shape = currentPiece.shape;
        for (let r = 0; r < shape.length; r++) {
            for (let c = 0; c < shape[r].length; c++) {
                if (!shape[r][c]) continue;
                const bx = currentPiece.x + c;
                const by = currentPiece.y + r;
                if (by >= 0 && by < ROWS && bx >= 0 && bx < COLS) {
                    board[by][bx] = currentPiece.type;
                }
            }
        }
        currentPiece = null;
        checkLines();
    }

    function checkLines() {
        const fullRows = [];
        for (let r = 0; r < ROWS; r++) {
            if (board[r].every(cell => cell !== null)) {
                fullRows.push(r);
            }
        }

        if (fullRows.length > 0) {
            clearingRows = fullRows;
            clearAnimStart = performance.now();
            // Trigger shake
            triggerShake();
        } else {
            spawnPiece();
        }
    }

    function finishClearLines() {
        const count = clearingRows.length;
        // Remove full rows
        for (const row of clearingRows.sort((a, b) => b - a)) {
            board.splice(row, 1);
            board.unshift(new Array(COLS).fill(null));
        }

        // Update score
        const gained = (SCORE_TABLE[count] || count * 100) * level;
        score += gained;
        totalLines += count;
        const newLevel = Math.floor(totalLines / 10) + 1;
        if (newLevel !== level) {
            level = newLevel;
        }
        updateScoreDisplay();
        clearingRows = [];
        spawnPiece();
    }

    // ===== 特效 =====
    function triggerShake() {
        canvasContainer.classList.remove('shake');
        // force reflow
        void canvasContainer.offsetWidth;
        canvasContainer.classList.add('shake');
        canvasContainer.addEventListener('animationend', () => {
            canvasContainer.classList.remove('shake');
        }, { once: true });
    }

    // ===== 分数显示 =====
    function updateScoreDisplay() {
        animateStatValue(scoreEl, score);
        animateStatValue(levelEl, level);
        animateStatValue(linesEl, totalLines);
    }

    function animateStatValue(el, value) {
        el.textContent = value;
        el.classList.remove('pop');
        void el.offsetWidth;
        el.classList.add('pop');
    }

    // ===== 渲染 =====
    function drawBlock(context, x, y, type, size, alpha = 1) {
        const baseColor = COLORS[type];
        const glow = GLOW_COLORS[type];

        context.globalAlpha = alpha;

        // Main fill
        context.fillStyle = baseColor;
        context.fillRect(x * size + 1, y * size + 1, size - 2, size - 2);

        // Inner highlight (top-left)
        context.fillStyle = 'rgba(255,255,255,0.18)';
        context.fillRect(x * size + 1, y * size + 1, size - 2, 3);
        context.fillRect(x * size + 1, y * size + 1, 3, size - 2);

        // Inner shadow (bottom-right)
        context.fillStyle = 'rgba(0,0,0,0.20)';
        context.fillRect(x * size + 1, y * size + size - 4, size - 2, 3);
        context.fillRect(x * size + size - 4, y * size + 1, 3, size - 2);

        // Glow border
        context.strokeStyle = glow;
        context.lineWidth = 1;
        context.strokeRect(x * size + 0.5, y * size + 0.5, size - 1, size - 1);

        context.globalAlpha = 1;
    }

    function drawGrid() {
        ctx.strokeStyle = 'rgba(148, 163, 184, 0.06)';
        ctx.lineWidth = 0.5;
        for (let c = 1; c < COLS; c++) {
            ctx.beginPath();
            ctx.moveTo(c * BLOCK_SIZE, 0);
            ctx.lineTo(c * BLOCK_SIZE, ROWS * BLOCK_SIZE);
            ctx.stroke();
        }
        for (let r = 1; r < ROWS; r++) {
            ctx.beginPath();
            ctx.moveTo(0, r * BLOCK_SIZE);
            ctx.lineTo(COLS * BLOCK_SIZE, r * BLOCK_SIZE);
            ctx.stroke();
        }
    }

    function drawBoard() {
        for (let r = 0; r < ROWS; r++) {
            for (let c = 0; c < COLS; c++) {
                if (board[r][c]) {
                    // If this row is being cleared, do flashing
                    if (clearingRows.includes(r)) {
                        const elapsed = performance.now() - clearAnimStart;
                        const flashCount = 3;
                        const flashCycle = CLEAR_ANIM_DURATION / flashCount;
                        const phase = (elapsed % flashCycle) / flashCycle;
                        const alpha = phase < 0.5 ? 1 : 0.15;
                        drawBlock(ctx, c, r, board[r][c], BLOCK_SIZE, alpha);
                    } else {
                        drawBlock(ctx, c, r, board[r][c], BLOCK_SIZE);
                    }
                }
            }
        }
    }

    function drawCurrentPiece() {
        if (!currentPiece) return;
        const shape = currentPiece.shape;
        for (let r = 0; r < shape.length; r++) {
            for (let c = 0; c < shape[r].length; c++) {
                if (!shape[r][c]) continue;
                drawBlock(ctx, currentPiece.x + c, currentPiece.y + r, currentPiece.type, BLOCK_SIZE);
            }
        }
    }

    function drawGhost() {
        if (!currentPiece) return;
        const ghostY = getGhostY();
        if (ghostY === currentPiece.y) return;

        const shape = currentPiece.shape;
        const color = COLORS[currentPiece.type];

        ctx.globalAlpha = 0.18;
        for (let r = 0; r < shape.length; r++) {
            for (let c = 0; c < shape[r].length; c++) {
                if (!shape[r][c]) continue;
                const bx = currentPiece.x + c;
                const by = ghostY + r;
                ctx.fillStyle = color;
                ctx.fillRect(bx * BLOCK_SIZE + 1, by * BLOCK_SIZE + 1, BLOCK_SIZE - 2, BLOCK_SIZE - 2);
                ctx.strokeStyle = color;
                ctx.lineWidth = 1;
                ctx.strokeRect(bx * BLOCK_SIZE + 0.5, by * BLOCK_SIZE + 0.5, BLOCK_SIZE - 1, BLOCK_SIZE - 1);
            }
        }
        ctx.globalAlpha = 1;
    }

    function drawNextPiece() {
        nextCtx.clearRect(0, 0, nextCanvas.width, nextCanvas.height);
        if (!nextPiece) return;

        const shape = nextPiece.shape;
        const rows = shape.length;
        const cols = shape[0].length;
        const previewSize = 24;
        const offsetX = (nextCanvas.width - cols * previewSize) / 2;
        const offsetY = (nextCanvas.height - rows * previewSize) / 2;

        for (let r = 0; r < rows; r++) {
            for (let c = 0; c < cols; c++) {
                if (!shape[r][c]) continue;
                const color = COLORS[nextPiece.type];
                const glow = GLOW_COLORS[nextPiece.type];
                const px = offsetX + c * previewSize;
                const py = offsetY + r * previewSize;

                nextCtx.fillStyle = color;
                nextCtx.fillRect(px + 1, py + 1, previewSize - 2, previewSize - 2);

                // highlight
                nextCtx.fillStyle = 'rgba(255,255,255,0.18)';
                nextCtx.fillRect(px + 1, py + 1, previewSize - 2, 2);
                nextCtx.fillRect(px + 1, py + 1, 2, previewSize - 2);

                // shadow
                nextCtx.fillStyle = 'rgba(0,0,0,0.20)';
                nextCtx.fillRect(px + 1, py + previewSize - 3, previewSize - 2, 2);
                nextCtx.fillRect(px + previewSize - 3, py + 1, 2, previewSize - 2);

                // glow border
                nextCtx.strokeStyle = glow;
                nextCtx.lineWidth = 1;
                nextCtx.strokeRect(px + 0.5, py + 0.5, previewSize - 1, previewSize - 1);
            }
        }
    }

    function render() {
        // Clear
        ctx.fillStyle = '#0f172a';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        drawGrid();
        drawBoard();

        if (gameState === STATE.PLAYING && clearingRows.length === 0) {
            drawGhost();
            drawCurrentPiece();
        }

        drawNextPiece();
    }

    // ===== 游戏循环 =====
    function getDropInterval() {
        const idx = Math.min(level - 1, LEVEL_SPEEDS.length - 1);
        return LEVEL_SPEEDS[idx];
    }

    function gameLoop(timestamp) {
        if (gameState !== STATE.PLAYING) return;

        // Handle line clear animation
        if (clearingRows.length > 0) {
            const elapsed = timestamp - clearAnimStart;
            if (elapsed >= CLEAR_ANIM_DURATION) {
                finishClearLines();
            }
            render();
            animationId = requestAnimationFrame(gameLoop);
            return;
        }

        // Automatic drop
        if (timestamp - lastDropTime >= getDropInterval()) {
            if (!movePiece(0, 1)) {
                lockPiece();
            }
            lastDropTime = timestamp;
        }

        render();
        animationId = requestAnimationFrame(gameLoop);
    }

    // ===== 游戏控制 =====
    function startGame() {
        createBoard();
        score = 0;
        level = 1;
        totalLines = 0;
        clearingRows = [];
        bag = [];
        currentPiece = null;
        nextPiece = null;
        updateScoreDisplay();
        spawnPiece();

        gameState = STATE.PLAYING;
        lastDropTime = performance.now();
        gameOverlay.classList.add('hidden');

        btnStart.disabled = true;
        btnPause.disabled = false;
        btnRestart.disabled = false;
        btnPause.textContent = '暂停';

        if (animationId) cancelAnimationFrame(animationId);
        animationId = requestAnimationFrame(gameLoop);
    }

    function togglePause() {
        if (gameState === STATE.PLAYING) {
            gameState = STATE.PAUSED;
            btnPause.textContent = '继续';
            overlayTitle.textContent = '游戏暂停';
            overlaySubtitle.textContent = '按 继续 或 P 键恢复';
            gameOverlay.classList.remove('hidden');
        } else if (gameState === STATE.PAUSED) {
            gameState = STATE.PLAYING;
            btnPause.textContent = '暂停';
            gameOverlay.classList.add('hidden');
            lastDropTime = performance.now();
            animationId = requestAnimationFrame(gameLoop);
        }
    }

    function gameOver() {
        gameState = STATE.GAMEOVER;
        overlayTitle.textContent = '游戏结束';
        overlaySubtitle.textContent = `最终分数: ${score}`;
        gameOverlay.classList.remove('hidden');
        btnStart.disabled = false;
        btnStart.textContent = '重新开始';
        btnPause.disabled = true;
        btnRestart.disabled = true;
    }

    // ===== 输入处理 =====
    const keyState = {};

    document.addEventListener('keydown', (e) => {
        if (keyState[e.code]) return; // prevent key repeat for some actions
        keyState[e.code] = true;

        switch (e.code) {
            case 'ArrowLeft':
                e.preventDefault();
                movePiece(-1, 0);
                break;
            case 'ArrowRight':
                e.preventDefault();
                movePiece(1, 0);
                break;
            case 'ArrowDown':
                e.preventDefault();
                if (movePiece(0, 1)) {
                    score += 1; // soft drop bonus
                    updateScoreDisplay();
                }
                break;
            case 'ArrowUp':
                e.preventDefault();
                rotatePiece();
                break;
            case 'Space':
                e.preventDefault();
                hardDrop();
                break;
            case 'KeyP':
                e.preventDefault();
                if (gameState === STATE.PLAYING || gameState === STATE.PAUSED) {
                    togglePause();
                }
                break;
        }
    });

    document.addEventListener('keyup', (e) => {
        keyState[e.code] = false;
    });

    // Allow key repeat for movement (re-enable after short delay)
    document.addEventListener('keydown', (e) => {
        if (['ArrowLeft', 'ArrowRight', 'ArrowDown'].includes(e.code)) {
            keyState[e.code] = false;
        }
    });

    // ===== 按钮事件 =====
    btnStart.addEventListener('click', startGame);
    btnPause.addEventListener('click', togglePause);
    btnRestart.addEventListener('click', startGame);

    // ===== 触摸控制 =====
    function bindTouch(id, handler) {
        const el = document.getElementById(id);
        if (!el) return;
        el.addEventListener('touchstart', (e) => {
            e.preventDefault();
            handler();
        });
        el.addEventListener('mousedown', (e) => {
            e.preventDefault();
            handler();
        });
    }

    bindTouch('touchLeft', () => movePiece(-1, 0));
    bindTouch('touchRight', () => movePiece(1, 0));
    bindTouch('touchDown', () => {
        if (movePiece(0, 1)) {
            score += 1;
            updateScoreDisplay();
        }
    });
    bindTouch('touchRotate', () => rotatePiece());
    bindTouch('touchDrop', () => hardDrop());

    // ===== 初始化渲染 =====
    createBoard();
    render();

})();
