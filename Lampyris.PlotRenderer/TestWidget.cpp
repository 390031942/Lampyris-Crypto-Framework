#include "TestWidget.h"
#include "MathUtil.h"

TestWidget::TestWidget(QWidget *parent)
	: QWidget(parent), 
	m_candleChart(new CandleChartWidget(this)), 
	m_volumeChart(new VolumeChartWidget(this)) {
	QObject::connect(&api, &BinanceAPI::dataFetched, this, &TestWidget::onDataFetched);
	api.fetchKlines("BTCUSDT", "1m", 1000);

	this->installEventFilter(this->m_candleChart);
	this->installEventFilter(this->m_volumeChart);
}

TestWidget::~TestWidget()
{}

void TestWidget::handleMouseMove(QPoint mousePos) {
	if (!m_context.dataList.empty()) {
		int widgetWidth = width();
		int candleWidth = m_context.width;

		int index = mousePos.x() / width();
		m_context.focusIndex = index + m_context.startIndex;
	}
}

void TestWidget::handleKeyArrowLeftOrRight(int key) {
	if (m_context.dataList.empty()) {
		return;
	}
	if (key == Qt::Key::Key_Left) {
		if (m_context.focusIndex == -1) {
			m_context.focusIndex = m_context.endIndex;
		}
		else {
			m_context.focusIndex = m_context.focusIndex - 1;
		}

		if (m_context.focusIndex < 0 && !m_context.isFullData) {
			// 请求新的历史数据
			m_context.isWaitingHistoryData = true;
			api.fetchKlinesFromEndTime("BTCUSDT", "1m", 100, m_context.dataList.front()->dateTime);
			m_context.expectedSize = 100;
		}
		m_context.startIndex = std::max(0, m_context.startIndex - 1);
	}
	else if (key == Qt::Key::Key_Right) {
		if (m_context.focusIndex == -1) {
			m_context.focusIndex = m_context.endIndex;
		}
		else {
			m_context.focusIndex = std::min((int)m_context.dataList.size() - 1, m_context.focusIndex + 1);
		}
	}
}

void TestWidget::handleKeyArrowUpOrDown(int key) {
	if (m_context.dataList.empty()) {
		return;
	}

	int targetWidthArrayIndex = std::clamp(key == Qt::Key::Key_Up ?
		                                   m_widthArrayIndex + 1 : m_widthArrayIndex - 1, 
		                                   0, (int)widthArray.size() - 1);

	if (targetWidthArrayIndex != m_widthArrayIndex) {
		m_widthArrayIndex = targetWidthArrayIndex;
		m_context.width = widthArray[targetWidthArrayIndex];
		int expectedCount = std::floor(width() / m_context.width);
		int currentCount = m_context.endIndex - m_context.startIndex + 1;
		int diffCount = expectedCount - currentCount;
		
		// 重新计算startIndex和endIndex
		if (m_context.focusIndex == -1) {
			m_context.startIndex -= diffCount;

			if (m_context.startIndex < 0) {
				// 请求新的历史数据
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("BTCUSDT", "1m", std::min(MAX_LIMIT, -m_context.startIndex), m_context.dataList.front()->dateTime);
				m_context.expectedSize = -m_context.startIndex;
			}
		}
		else {
			double ratio = (double)(m_context.focusIndex - m_context.startIndex) / (double)currentCount;
			int leftSideExpectedCount = ratio * expectedCount;
			int rightSideExpectedCount = expectedCount - leftSideExpectedCount;

			if (m_context.endIndex + rightSideExpectedCount >= m_context.dataList.size()) {
				int exceedCount = m_context.dataList.size() - (m_context.endIndex + rightSideExpectedCount) + 1;
				rightSideExpectedCount -= exceedCount;
				leftSideExpectedCount += exceedCount;
			}

			m_context.startIndex -= leftSideExpectedCount;
			if (m_context.startIndex < 0) {
				// 请求新的历史数据
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("BTCUSDT", "1m", std::min(MAX_LIMIT, m_context.startIndex), m_context.dataList.front()->dateTime);
				m_context.expectedSize = m_context.startIndex;
			}
		}
	}
}

void TestWidget::recalculateContextParam() {
	for (int i = m_context.startIndex; i < m_context.endIndex; i++) {
		auto& data = m_context.dataList[i];
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

	m_context.gridMaxPrice = MathUtil::ceilModulo(m_context.maxPrice * 1.005, 4, gridRowCount);
	m_context.gridMinPrice = MathUtil::floorModulo(m_context.minPrice * 0.995, 4, gridRowCount);
}

void TestWidget::onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList) {
	if (dataList.size() == 0) {
		return;
	}

	if (m_context.expectedSize > dataList.size()) {
		m_context.isFullData = true;
	}

	m_context.expectedSize = 0;

	m_context.dataList.insert(m_context.dataList.begin(), dataList.begin(), dataList.end());
	m_candleChart->update();
	m_volumeChart->update();
}
