// Project Include(s)
#include "ui_betterchart.h"
#include "BetterCandleQuoteChart.h"
#include "BetterCandleChartWidget.h"
#include "BetterVolumeChartWidget.h"

// QT Include(s)
#include <QSplitter>

BetterCandleQuoteChart::BetterCandleQuoteChart(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::BetterChart) {
    ui->setupUi(this);
    auto candleChart = new BetterCandleChartWidget(this);
    candleChart->setObjectName(tr("kline"));
    candleChart->setFocusPolicy(Qt::StrongFocus);

    auto volumeChart = new BetterVolumeChartWidget(this);
    volumeChart->setFocusPolicy(Qt::StrongFocus);

    QSplitter *splitterMain = new QSplitter(Qt::Vertical, 0); //新建主分割窗口，水平分割
    QSplitter *splitterLeft = new QSplitter(Qt::Vertical, splitterMain);
    QSplitter *splitterRight = new QSplitter(Qt::Vertical, splitterMain);

    splitterMain->setHandleWidth(1);
    splitterLeft->addWidget(candleChart);
    splitterRight->addWidget(volumeChart);
    this->setCentralWidget(splitterMain);

    m_chartList.push_back(candleChart);
    m_chartList.push_back(volumeChart);

    resize(1280, 720);
}

BetterCandleQuoteChart::~BetterCandleQuoteChart() {
    delete ui;
}
