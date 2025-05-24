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
    int startX;    // ��ʼλ��
    int width;     // ���
};

// TableHeader ��, ���ڱ�ʾһ����ͷ�ؼ���
// �����ͷ�������FieldGroup��ÿ��fieldGroupΪһ��Column
// һ��Column���԰������Field�����Field֮��ʹ��"/"�ָ�,
// һ��Field��������Ϊ������/�������򣬶��ڿ������Field������Ժ��ܹ����α�Ϊ����->����->����
// UI�����ϣ���ͷ��ˮƽ���֡�����Ҳ඼�е���Spacer����ÿһ��fieldGroupContainer����������
// fieldGroupContainer��Ҳ�ǳ�ˮƽ���֣�������ʾһ������fieldContainer�����fieldContainer֮����"/"�ָ���
class TableHeader : public QWidget {
    Q_OBJECT

private:
    // ��װ�ֶ���Ϣ�Ľṹ��
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