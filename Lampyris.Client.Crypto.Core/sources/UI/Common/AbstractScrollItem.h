#pragma once

// QT Include(s)
#include <QWidget>
#include <QDebug>

#include <concepts>
#include <functional>
#include <type_traits>

class AbstractDataObject {
public:
    virtual ~AbstractDataObject() = default;

    // 定义一个虚函数，用于获取数据的描述（可以根据需求扩展）
    virtual QString getDescription() const = 0;
};

class AbstractScrollItem : public QWidget {
    Q_OBJECT

public:
    explicit AbstractScrollItem(QWidget* parent = nullptr) : QWidget(parent) {}
    virtual ~AbstractScrollItem() = default;

    // 设置数据
    virtual void setData(const AbstractDataObject& data) = 0;

    // 设置索引（可选，用于调试或其他用途）
    virtual void setIndex(int index) {}
};

// 定义 ScrollItemType Concept
template <typename T>
concept ScrollItemType = std::is_base_of_v<AbstractScrollItem, T>&& requires(T item, QWidget* parent) {
    { new T(parent) } -> std::convertible_to<AbstractScrollItem*>;
};
