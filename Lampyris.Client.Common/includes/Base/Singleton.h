#pragma once

// STD Include(s)
#include <type_traits>
#include <concepts>

// QT Include(s)
#include <QObject>

template<class T>
class Singleton {
public:
	static T* getInstance() {
		static T t;
		return &t;
	}
};

// 定义一个宏，用于生成单例类
#define DECLARE_SINGLETON_QOBJECT(ClassName) \
public:                                      \
    static ClassName* getInstance() {        \
        static ClassName instance;           \
        return &instance;                    \
    }               