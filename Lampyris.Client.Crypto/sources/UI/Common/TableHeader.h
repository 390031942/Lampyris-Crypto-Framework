#pragma once

// QT Include(s)
#include <QWidget>
#include <QHBoxLayout>
#include <QLabel>
#include <QMouseEvent>
#include <QResizeEvent>
#include <QEnterEvent>
#include <QEvent>
#include <QTimer>
#include <QSpacerItem>
#include <QPixmap>

// STD Include(s)
#include <vector>
#include <tuple>
#include <unordered_map>

// Project Include(s)
#include "Const/DataSortingOrder.h"

class TableHeaderDefinition {
    using Field = std::tuple<QString, bool, double>;
    using FieldVector = std::vector<Field>;
public:
    TableHeaderDefinition& startFieldGroup(double ratioOrWidth);
    TableHeaderDefinition& addField(const QString& fieldName, bool sortable);
    void end();
private:
    std::vector<FieldVector> m_definition;

    friend class TableHeader;
};

struct TableColumnWidthInfo {
    int startX;    // 起始位置
    int width;     // 宽度
};

// TableHeader 类, 用于表示一个表头控件。
// 这个表头包含多个FieldGroup，每个fieldGroup为一个Column
// 一个Column可以包含多个Field，多个Field之间使用"/"分隔,
// 一个Field可以设置为可排序/不可排序，对于可排序的Field，点击以后能够依次变为降序->升序->无序。
// UI布局上，表头呈水平布局。左侧右侧都有弹性Spacer区域。每一个fieldGroupContainer从左到右排列
// fieldGroupContainer里也是呈水平布局，里面显示一个或多个fieldContainer，多个fieldContainer之间用"/"分隔。
class TableHeader : public QWidget {
    Q_OBJECT

private:
    // 封装字段信息的结构体
    struct FieldInfo {
        QLabel* fieldLabel;
        QLabel* arrowLabel;
        bool sortable;
        DataSortingOrder sortOrder;
    };

    static std::unordered_map<DataSortingOrder, QPixmap> ms_iconMap;
public:
    explicit TableHeader(QWidget* parent = nullptr);
    void setHeaderDefinition(const TableHeaderDefinition& definition);

signals:
    void sortRequested(const QString& field, DataSortingOrder sortOrder);
    void columnWidthResized(const std::vector<TableColumnWidthInfo>& widthInfoList);
protected:
    void enterEvent(QEnterEvent* event) override;
    void leaveEvent(QEvent* event) override;
    void resizeEvent(QResizeEvent* event) override;
    bool eventFilter(QObject* obj, QEvent* event) override;

private:
    QHBoxLayout* m_layout;
    FieldInfo* m_sortingField;
    std::unordered_map<QLabel*, FieldInfo> m_fieldInfoMap;
    std::vector<std::pair<QWidget*, double>> m_fieldGroupWidths;
    void createFieldGroup(const TableHeaderDefinition::FieldVector& fieldGroup);
    void adjustWidth();
    void updateArrow(QLabel* fieldLabel, const QPixmap& pixmap);
};