#pragma once

// Project Include(s)
#include <vector>
#include <algorithm>
#include <functional>

// 通用模板类
template<typename... Args>
class Delegate {
    /*
     * 函数指针的类型
    */
    using Func = std::function<void(Args...)>;
    /*
     * 定义一个类型用于标识函数
    */
    using Identifier = int;

    struct FuncEntry {
        Func       func;
        Identifier id;
    };

public:
    /*
     * 添加监听函数
     * 参数{func}:待添加的函数的地址
     * 返回值:函数唯一标识符
     */
    Identifier operator+=(Func func) {
        Identifier newId = m_increment++;
        m_funcList.push_back(FuncEntry{ std::move(func), newId });
        return newId;
    }
    /*
     * 移除监听函数
     * 参数{Identifier}:函数唯一标识符
     */
    bool operator-=(Identifier id) {
        auto it = std::remove_if(m_funcList.begin(), m_funcList.end(), [id](const FuncEntry& entry) { return entry.id == id; });
        if (it != m_funcList.end()) {
            m_funcList.erase(it, m_funcList.end());
            return true;
        }
        return false;
    }
    /*
     * 调用所有注册的函数
     */
    void operator()(Args... args) {
        for (auto& funcEntry : m_funcList) {
            funcEntry.func(args...);
        }
    }

private:
    std::vector<FuncEntry> m_funcList;
    int                    m_increment = 0;
};

// 特化版本：无参数情况
template<>
class Delegate<void> {
    using Func = std::function<void()>;
    using Identifier = int;

    struct FuncEntry {
        Func       func;
        Identifier id;
    };

public:
    /*
     * 添加监听函数
     * 参数{func}:待添加的函数的地址
     * 返回值:函数唯一标识符
     */
    Identifier operator+=(Func func) {
        Identifier newId = m_increment++;
        m_funcList.push_back(FuncEntry{ std::move(func), newId });
        return newId;
    }
    /*
     * 移除监听函数
     * 参数{Identifier}:函数唯一标识符
     */
    bool operator-=(Identifier id) {
        auto it = std::remove_if(m_funcList.begin(), m_funcList.end(), [id](const FuncEntry& entry) { return entry.id == id; });
        if (it != m_funcList.end()) {
            m_funcList.erase(it, m_funcList.end());
            return true;
        }
        return false;
    }
    /*
     * 调用所有注册的函数
     */
    void operator()() {
        for (auto& funcEntry : m_funcList) {
            funcEntry.func();
        }
    }

private:
    std::vector<FuncEntry> m_funcList;
    int                    m_increment = 0;
};