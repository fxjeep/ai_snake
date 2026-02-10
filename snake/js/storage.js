/* ========================================
   数据存储模块
   ======================================== */

const Storage = {
    KEYS: {
        USERS: 'snake_users',
        CURRENT_USER: 'snake_current_user',
        LEADERBOARD: 'snake_leaderboard'
    },

    /* ========================================
       通用存储方法
       ======================================== */
    
    /**
     * 获取存储数据
     * @param {string} key - 存储键名
     * @param {*} defaultValue - 默认值
     * @returns {*} 存储的数据或默认值
     */
    get(key, defaultValue = null) {
        try {
            const data = localStorage.getItem(key);
            return data ? JSON.parse(data) : defaultValue;
        } catch (e) {
            console.error('Storage get error:', e);
            return defaultValue;
        }
    },

    /**
     * 设置存储数据
     * @param {string} key - 存储键名
     * @param {*} value - 要存储的数据
     */
    set(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
        } catch (e) {
            console.error('Storage set error:', e);
        }
    },

    /**
     * 删除存储数据
     * @param {string} key - 存储键名
     */
    remove(key) {
        try {
            localStorage.removeItem(key);
        } catch (e) {
            console.error('Storage remove error:', e);
        }
    },

    /* ========================================
       用户相关方法
       ======================================== */
    
    /**
     * 获取所有用户
     * @returns {Object} 用户对象 {username: userData}
     */
    getAllUsers() {
        return this.get(this.KEYS.USERS, {});
    },

    /**
     * 获取用户
     * @param {string} username - 用户名
     * @returns {Object|null} 用户数据
     */
    getUser(username) {
        const users = this.getAllUsers();
        return users[username] || null;
    },

    /**
     * 保存用户
     * @param {string} username - 用户名
     * @param {Object} userData - 用户数据
     */
    saveUser(username, userData) {
        const users = this.getAllUsers();
        users[username] = userData;
        this.set(this.KEYS.USERS, users);
    },

    /**
     * 获取当前登录用户
     * @returns {Object|null} 当前用户数据
     */
    getCurrentUser() {
        return this.get(this.KEYS.CURRENT_USER, null);
    },

    /**
     * 设置当前登录用户
     * @param {Object|null} user - 用户数据
     */
    setCurrentUser(user) {
        if (user) {
            this.set(this.KEYS.CURRENT_USER, user);
        } else {
            this.remove(this.KEYS.CURRENT_USER);
        }
    },

    /* ========================================
       游戏数据方法
       ======================================== */
    
    /**
     * 更新用户最高分
     * @param {string} username - 用户名
     * @param {number} score - 分数
     * @returns {boolean} 是否为新纪录
     */
    updateHighScore(username, score) {
        const user = this.getUser(username);
        if (!user) return false;
        
        const isNewRecord = score > (user.highScore || 0);
        if (isNewRecord) {
            user.highScore = score;
            user.gamesPlayed = (user.gamesPlayed || 0) + 1;
            this.saveUser(username, user);
            this.updateLeaderboard(username, score);
        } else {
            user.gamesPlayed = (user.gamesPlayed || 0) + 1;
            this.saveUser(username, user);
        }
        
        return isNewRecord;
    },

    /**
     * 更新排行榜
     * @param {string} username - 用户名
     * @param {number} score - 分数
     */
    updateLeaderboard(username, score) {
        let leaderboard = this.get(this.KEYS.LEADERBOARD, []);
        
        // 移除该用户的旧记录
        leaderboard = leaderboard.filter(entry => entry.username !== username);
        
        // 添加新记录
        leaderboard.push({ username, score });
        
        // 按分数排序，取前10
        leaderboard.sort((a, b) => b.score - a.score);
        leaderboard = leaderboard.slice(0, 10);
        
        this.set(this.KEYS.LEADERBOARD, leaderboard);
    },

    /**
     * 获取排行榜
     * @returns {Array} 排行榜数据
     */
    getLeaderboard() {
        return this.get(this.KEYS.LEADERBOARD, []);
    },

    /**
     * 记录登录时间
     * @param {string} username - 用户名
     */
    recordLoginTime(username) {
        const user = this.getUser(username);
        if (!user) return;
        
        user.lastLoginTime = new Date().toISOString();
        if (!user.loginHistory) {
            user.loginHistory = [];
        }
        user.loginHistory.push(user.lastLoginTime);
        // 只保留最近10次登录记录
        if (user.loginHistory.length > 10) {
            user.loginHistory = user.loginHistory.slice(-10);
        }
        
        this.saveUser(username, user);
    },

    /* ========================================
       密码哈希（简单实现）
       ======================================== */
    
    /**
     * 简单的密码哈希
     * @param {string} password - 原始密码
     * @returns {string} 哈希后的密码
     */
    hashPassword(password) {
        // 注意：这是一个非常简单的哈希，仅用于演示目的
        // 生产环境应使用更安全的方法
        let hash = 0;
        for (let i = 0; i < password.length; i++) {
            const char = password.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }
        return 'h_' + Math.abs(hash).toString(16);
    }
};

// 导出模块（如果是模块化环境）
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Storage;
}
