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

// ����һ���꣬�������ɵ�����
#define DECLARE_SINGLETON_QOBJECT(ClassName) \
public:                                      \
    static ClassName* getInstance() {        \
        static ClassName instance;           \
        return &instance;                    \
    }               