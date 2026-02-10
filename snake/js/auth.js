/* ========================================
   用户认证模块
   ======================================== */

const Auth = {
    /**
     * 注册新用户
     * @param {string} username - 用户名
     * @param {string} password - 密码
     * @returns {Object} {success: boolean, message: string}
     */
    register(username, password) {
        // 验证输入
        if (!username || !password) {
            return { success: false, message: '请输入用户名和密码' };
        }

        if (username.length < 2 || username.length > 20) {
            return { success: false, message: '用户名长度需要2-20个字符' };
        }

        if (password.length < 4) {
            return { success: false, message: '密码长度至少4个字符' };
        }

        // 检查用户是否已存在
        if (Storage.getUser(username)) {
            return { success: false, message: '用户名已存在' };
        }

        // 创建新用户
        const userData = {
            username: username,
            password: Storage.hashPassword(password),
            highScore: 0,
            gamesPlayed: 0,
            createdAt: new Date().toISOString(),
            lastLoginTime: null,
            loginHistory: []
        };

        Storage.saveUser(username, userData);

        return { success: true, message: '注册成功！' };
    },

    /**
     * 用户登录
     * @param {string} username - 用户名
     * @param {string} password - 密码
     * @returns {Object} {success: boolean, message: string, user?: Object}
     */
    login(username, password) {
        // 验证输入
        if (!username || !password) {
            return { success: false, message: '请输入用户名和密码' };
        }

        // 获取用户
        const user = Storage.getUser(username);
        if (!user) {
            return { success: false, message: '用户名不存在' };
        }

        // 验证密码
        if (user.password !== Storage.hashPassword(password)) {
            return { success: false, message: '密码错误' };
        }

        // 记录登录时间
        Storage.recordLoginTime(username);

        // 设置当前用户（不包含密码）
        const currentUser = {
            username: user.username,
            highScore: user.highScore,
            gamesPlayed: user.gamesPlayed,
            isGuest: false
        };
        Storage.setCurrentUser(currentUser);

        return { success: true, message: '登录成功！', user: currentUser };
    },

    /**
     * 游客模式登录
     * @returns {Object} 游客用户数据
     */
    loginAsGuest() {
        const guestUser = {
            username: '游客',
            highScore: 0,
            gamesPlayed: 0,
            isGuest: true
        };
        Storage.setCurrentUser(guestUser);
        return guestUser;
    },

    /**
     * 退出登录
     */
    logout() {
        Storage.setCurrentUser(null);
    },

    /**
     * 获取当前用户
     * @returns {Object|null} 当前用户数据
     */
    getCurrentUser() {
        return Storage.getCurrentUser();
    },

    /**
     * 检查是否已登录
     * @returns {boolean}
     */
    isLoggedIn() {
        return !!this.getCurrentUser();
    },

    /**
     * 检查是否为游客
     * @returns {boolean}
     */
    isGuest() {
        const user = this.getCurrentUser();
        return user ? user.isGuest : false;
    },

    /**
     * 更新当前用户分数
     * @param {number} score - 新分数
     * @returns {boolean} 是否为新纪录
     */
    updateScore(score) {
        const user = this.getCurrentUser();
        if (!user || user.isGuest) {
            // 游客模式只更新会话分数，不持久化
            if (user && score > user.highScore) {
                user.highScore = score;
                user.gamesPlayed++;
                Storage.setCurrentUser(user);
                return true;
            }
            if (user) {
                user.gamesPlayed++;
                Storage.setCurrentUser(user);
            }
            return false;
        }

        // 注册用户更新持久化分数
        const isNewRecord = Storage.updateHighScore(user.username, score);

        // 更新当前会话用户数据
        const updatedUser = Storage.getUser(user.username);
        const currentUser = {
            username: updatedUser.username,
            highScore: updatedUser.highScore,
            gamesPlayed: updatedUser.gamesPlayed,
            isGuest: false
        };
        Storage.setCurrentUser(currentUser);

        return isNewRecord;
    }
};

// 导出模块（如果是模块化环境）
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Auth;
}
