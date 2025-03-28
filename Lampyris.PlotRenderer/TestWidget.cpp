#include "TestWidget.h"
#include "MathUtil.h"

TestWidget::TestWidget(QWidget *parent)
	: QWidget(parent), 
	m_candleChart(new CandleChartWidget(this)), 
	m_volumeChart(new VolumeChartWidget(this)) {
	QObject::connect(&api, &BinanceAPI::dataFetched, this, &TestWidget::onDataFetched);
	api.fetchKlines("BTCUSDT", "1d", 1000);

	this->installEventFilter(this->m_candleChart);
	this->installEventFilter(this->m_volumeChart);
}

TestWidget::~TestWidget()
{}

void TestWidget::handleMouseMove(QPoint mousePos) {

}

void TestWidget::handleKeyArrowLeftOrRight() {

}

void TestWidget::handleKeyArrowUpOrDown() {

}

void TestWidget::onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList) {
	for (int i = 0; i < dataList.size(); i++) {
		auto& data = dataList[i];
		if (m_context.maxIndex == -1 || data->close > m_context.maxPrice) {
			m_context.maxIndex = i;
			m_context.maxPrice = data->close;
		}
		if (m_context.minIndex == -1 || data->close < m_context.minPrice) {
			m_context.minIndex = i;
			m_context.minPrice = data->close;
		}
	}

	// 计算grid最高价和最低价
	const int gridRowCount = 5;
	const int gridColCount = 4;

	m_context.gridMaxPrice = MathUtil::ceilModulo(m_context.maxPrice * 1.005,  4, gridRowCount);
	m_context.gridMinPrice = MathUtil::floorModulo(m_context.minPrice * 0.995, 4, gridRowCount);

	m_context.dataList.insert(m_context.dataList.begin(),dataList.begin(), dataList.end());
	m_candleChart->updateRenderContext(context);
	m_volumeChart->updateRenderContext(context);
}
