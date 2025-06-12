#pragma once

// QT Include(s)
#include <QWidget>
#include <QLayout>

// Project Include(s)
#include "UI/Common/MarketOverviewBarChartWidget.h"

class HomeWidget : public QWidget {
    Q_OBJECT

public:
    explicit HomeWidget(QWidget* parent = nullptr);
private:
    // �ǵ��ֲ���״ͼ
    MarketOverviewBarChartWidget* m_overviewBarChart;
    QVBoxLayout* m_layout;
};
