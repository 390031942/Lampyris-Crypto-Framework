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

    // ����һ���麯�������ڻ�ȡ���ݵ����������Ը���������չ��
    virtual QString getDescription() const = 0;
};

class AbstractScrollItem : public QWidget {
    Q_OBJECT

public:
    explicit AbstractScrollItem(QWidget* parent = nullptr) : QWidget(parent) {}
    virtual ~AbstractScrollItem() = default;

    // ��������
    virtual void setData(const AbstractDataObject& data) = 0;

    // ������������ѡ�����ڵ��Ի�������;��
    virtual void setIndex(int index) {}
};

// ���� ScrollItemType Concept
template <typename T>
concept ScrollItemType = std::is_base_of_v<AbstractScrollItem, T>&& requires(T item, QWidget* parent) {
    { new T(parent) } -> std::convertible_to<AbstractScrollItem*>;
};
