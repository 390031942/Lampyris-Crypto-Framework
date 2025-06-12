// Project Include(sv)
#include "HomeWidget.h"

HomeWidget::HomeWidget(QWidget* parent)
    : QWidget(parent), 
    m_layout(new QVBoxLayout(this)),
    m_overviewBarChart(new MarketOverviewBarChartWidget(this)) {
    m_layout->setContentsMargins(6, 6, 6, 6);
    m_layout->addWidget(m_overviewBarChart);
}