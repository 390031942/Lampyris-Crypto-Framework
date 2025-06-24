// Project Include(s)
#include "CandleQuoteDisplayWidget.h"
#include "MathUtil.h"
#include "PlotRenderer.h"

// QT Include(s)
#include <QLayout>
#include <QTimer>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonArray>
#include <qregularexpression.h>

const std::array<double, 13> CandleQuoteDisplayWidget::WIDTH_ARRAY =
{ 0.0625, 0.125, 0.25, 0.5, 0.7, 1, 2, 3, 4, 6, 12, 18, 24 };

// 事件过滤器
bool CandleQuoteDisplayWidget::eventFilter(QObject* watched, QEvent* event) {
	if (watched == m_candleChart) {
		if (event->type() == QEvent::MouseMove) {
			QMouseEvent* mouseEvent = static_cast<QMouseEvent*>(event);
			this->handleMouseMove(mouseEvent);
		}
		else if (event->type() == QEvent::MouseButtonPress) {
			QMouseEvent* mouseEvent = static_cast<QMouseEvent*>(event);
			qDebug() << "Mouse button pressed in child window:" << mouseEvent->button();
		}
		else if (event->type() == QEvent::KeyPress) {
			QKeyEvent* keyEvent = static_cast<QKeyEvent*>(event);
			qDebug() << "Key pressed in child window:" << keyEvent->key();
		}
	}
	// 默认处理
	return QWidget::eventFilter(watched, event);
}

void CandleQuoteDisplayWidget::resizeEvent(QResizeEvent* e) {
	if (!m_context.dataList.empty()) {
		QSize size = e->size();
		int width = size.width();

		int oldCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, e->oldSize().width() - this->m_context.gridScaleAreaWidth());
		int newCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, e->size().width() - this->m_context.gridScaleAreaWidth());

		if (oldCount != newCount && oldCount < m_context.dataList.size()) {
			m_context.startIndex = m_context.endIndex - newCount;
		}
		recalculateContextParam();
		this->repaintChart();
	}
	QWidget::resizeEvent(e);
}

void CandleQuoteDisplayWidget::keyPressEvent(QKeyEvent* e) {
	auto key = e->key();
	if (key == Qt::Key::Key_Left || key == Qt::Key::Key_Right) {
		this->handleKeyArrowLeftOrRight(key);
		this->repaintChart();
	}
	else if (key == Qt::Key::Key_Up || key == Qt::Key::Key_Down) {
		this->handleKeyArrowUpOrDown(key);
		this->repaintChart();
	}
	else if (key == Qt::Key::Key_Escape) {
		this->m_context.focusIndex = -1;
		this->repaintChart();
	}
	QWidget::keyPressEvent(e);
}

void CandleQuoteDisplayWidget::mouseMoveEvent(QMouseEvent* e) {
	this->handleMouseMove(e);
	QWidget::mouseMoveEvent(e);
}

void CandleQuoteDisplayWidget::handleMouseMove(QMouseEvent* e) {
	QPoint pos = e->pos();
	int x = pos.x();
	int focusIndex = m_context.focusIndex;

	// 计算focuxsIndex
	m_context.focusIndex = calculateFocusIndex(x, m_context.leftOffset, m_context.width, m_context.spacing, m_context.endIndex - m_context.startIndex) + m_context.startIndex;
	if (focusIndex != m_context.focusIndex) {
		qDebug() << m_context.focusIndex;
	}

	this->repaintChart();
}

int CandleQuoteDisplayWidget::calculateFocusIndex(double X, double leftOffset, double width, double spacing, int numKlines) {
	// 每个 K 线的总宽度
	double totalWidth = width + spacing;

	// 鼠标相对于 K 线区域的偏移量
	double relativeX = X - leftOffset;

	// 如果鼠标在 K 线区域的左侧
	if (relativeX < 0) {
		return 0;
	}

	// 如果鼠标在 K 线区域的右侧
	if (relativeX > numKlines * totalWidth) {
		return numKlines - 1;
	}

	// 大致的 K 线索引
	int approxIndex = floor(relativeX / totalWidth);

	// 左侧 K 线的中心位置
	double centerLeft = leftOffset + approxIndex * totalWidth + width / 2;

	// 右侧 K 线的中心位置
	double centerRight = centerLeft + totalWidth;

	// 判断鼠标离哪根 K 线的中心更近
	int focusIndex;
	if (fabs(X - centerLeft) <= fabs(X - centerRight)) {
		focusIndex = approxIndex;
	}
	else {
		focusIndex = approxIndex + 1;
	}

	// 确保索引合法
	if (focusIndex < 0) {
		focusIndex = 0;
	}
	else if (focusIndex >= numKlines) {
		focusIndex = numKlines - 1;
	}

	return focusIndex;
}

int CandleQuoteDisplayWidget::calculateCandleCount(double leftOffset, double width, double spacing, double windowWidth) {
	// 每根 K 线的总宽度
	double totalWidth = width + spacing;

	// 可用绘制宽度
	double availableWidth = windowWidth - leftOffset;

	// 如果可用宽度不足以显示任何 K 线
	if (availableWidth <= 0) {
		return 0;
	}

	// 计算最多显示的 K 线数量
	int candleCount = floor(availableWidth / totalWidth);

	return candleCount;
}

CandleQuoteDisplayWidget::CandleQuoteDisplayWidget(QWidget *parent)
	: QWidget(parent), 
	m_candleChart(new CandleChartWidget(this)), 
	m_volumeChart(new VolumeChartWidget(this)),
	m_betterSplitter(new BetterSplitter(Qt::Orientation::Vertical,this)) {

	// 设置 QSplitter 的 handle 样式
	m_betterSplitter->setStyleSheet("QSplitter::handle { background: gray; }");
	m_betterSplitter->setHandleWidth(1);

	QHBoxLayout* layout = new QHBoxLayout(this);
	layout->setContentsMargins(0, 0, 0, 0);
	this->setLayout(layout);
	layout->addWidget(m_betterSplitter);
	layout->addWidget(m_candleChart);

	m_betterSplitter->addWidget(m_candleChart);
	m_betterSplitter->addWidget(m_volumeChart);

	QObject::connect(&api, &BinanceAPI::dataFetched, this, &CandleQuoteDisplayWidget::onDataFetched);
	api.fetchKlines("NXPCUSDT", "1m", 1000);
	m_context.expectedSize = 1000;

	this->m_candleChart->installEventFilter(this);
	this->m_volumeChart->installEventFilter(this);

	PlotRenderer* renderer = new PlotRenderer;
	renderer->setRenderContext(&m_context);

	this->m_candleChart->setRenderer(renderer);
	this->m_volumeChart->setRenderer(renderer);

	this->setMouseTracking(true);
	this->m_candleChart->setMouseTracking(true);
	this->m_volumeChart->setMouseTracking(true);

	QTimer* timer = new QTimer(this);
	connect(timer, &QTimer::timeout, [this]() {
		this->m_candleChart->repaint();
		this->m_volumeChart->repaint();
	});

	m_context.minTick = getMinTick("NXPCUSDT");
	// timer->setInterval(30);
	// timer->start();
}

CandleQuoteDisplayWidget::~CandleQuoteDisplayWidget()
{}

void CandleQuoteDisplayWidget::handleMouseMove(QPoint mousePos) {
	if (!m_context.dataList.empty()) {
		int widgetWidth = width();
		int candleWidth = m_context.width;

		int index = mousePos.x() / width();
		m_context.focusIndex = index + m_context.startIndex;
	}
}

void CandleQuoteDisplayWidget::handleKeyArrowLeftOrRight(int key) {
	if (m_context.dataList.empty()) {
		return;
	}
	if (key == Qt::Key::Key_Left) {
		if (m_context.focusIndex == -1) {
			m_context.focusIndex = m_context.endIndex;
		}
		else {
			m_context.focusIndex = m_context.focusIndex - 1;
			if (m_context.focusIndex < m_context.startIndex) {
				m_context.startIndex -= 1;
				m_context.endIndex -= 1;
			}
		}

		if (m_context.focusIndex < 0) {
			if (!m_context.isFullData) {
				// 请求新的历史数据
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("NXPCUSDT", "1m", 100, m_context.dataList.front()->dateTime);
				m_context.expectedSize = 100;
				m_context.needAdjustFocusIndex = 100;
			}
			else {
				m_context.startIndex = 0;
				m_context.focusIndex = 0;
			}
		}
	}
	else if (key == Qt::Key::Key_Right) {
		if (m_context.focusIndex == -1) {
			m_context.focusIndex = m_context.startIndex;
		}
		else {
			m_context.focusIndex = std::min((int)m_context.dataList.size() - 1, m_context.focusIndex + 1);
			if (m_context.focusIndex >= m_context.endIndex) {
				if (m_context.endIndex < m_context.dataList.size()) {
					m_context.startIndex += 1;
					m_context.endIndex += 1;
				}
			}
		}
	}

	this->recalculateContextParam();
}

void CandleQuoteDisplayWidget::handleKeyArrowUpOrDown(int key) {
	if (m_context.dataList.empty()) {
		return;
	}

	int targetWidthArrayIndex = std::clamp(key == Qt::Key::Key_Up ?
		                                   m_widthArrayIndex + 1 : m_widthArrayIndex - 1, 
		                                   0, (int)WIDTH_ARRAY.size() - 1);

	if (targetWidthArrayIndex != m_widthArrayIndex) {
		m_widthArrayIndex = targetWidthArrayIndex;
		m_context.width = WIDTH_ARRAY[targetWidthArrayIndex];
		m_context.spacing = 0.5 * m_context.width;
		int expectedCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, width() - this->m_context.gridScaleAreaWidth());
		int currentCount = m_context.endIndex - m_context.startIndex;
		int diffCount = expectedCount - currentCount;
		
		// 重新计算startIndex和endIndex
		if (m_context.focusIndex == -1) {
			m_context.startIndex -= diffCount;

			if (m_context.startIndex < 0) {
				// 请求新的历史数据
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("NXPCUSDT", "1m", -m_context.startIndex, m_context.dataList.front()->dateTime);
				m_context.expectedSize = -m_context.startIndex;
				m_context.startIndex = 0;
			}
			else {
				this->recalculateContextParam();
				this->repaintChart();
			}
		}
		else {
			double ratio = currentCount >= 1 ? (double)(m_context.focusIndex - m_context.startIndex) / (double)(currentCount - 1) : 0.0;
			int leftSideExpectedCount = (1 - ratio) * diffCount;
			int rightSideExpectedCount = diffCount - leftSideExpectedCount;

			if (m_context.endIndex + rightSideExpectedCount > m_context.dataList.size()) {
				int exceedCount = (m_context.endIndex + rightSideExpectedCount) - m_context.dataList.size();
				rightSideExpectedCount -= exceedCount;
				leftSideExpectedCount += exceedCount;
			}

			m_context.startIndex -= leftSideExpectedCount;
			m_context.endIndex += rightSideExpectedCount;

			if (m_context.startIndex < 0) {
				// 请求新的历史数据
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("NXPCUSDT", "1m", -m_context.startIndex, m_context.dataList.front()->dateTime);
				m_context.expectedSize = -m_context.startIndex;
				m_context.startIndex = 0;
			}
			else {
				this->recalculateContextParam();
				this->repaintChart();
			}
		}
	}
}

void CandleQuoteDisplayWidget::recalculateContextParam() {
	if (m_context.dataList.empty()) {
		return;
	}

	int startIndex = !m_isFirstTime ? m_context.startIndex : 0;
	int endIndex   = !m_isFirstTime ? m_context.endIndex : m_context.dataList.size() - 1;

	m_context.maxIndex = -1;
	m_context.minIndex = -1;
	m_context.maxPrice = 0;
	m_context.minPrice = 0;

	for (int i = startIndex; i < endIndex; i++) {
		auto& data = m_context.dataList[i];
		if (m_context.maxIndex == -1 || data->high > m_context.maxPrice) {
			m_context.maxIndex = i;
			m_context.maxPrice = data->high;
		}
		if (m_context.minIndex == -1 || data->low < m_context.minPrice) {
			m_context.minIndex = i;
			m_context.minPrice = data->low;
		}
	}
	 
	// 计算grid最高价和最低价
	const int gridRowCount = 5; 
	const int gridColCount = 4;

 	m_context.gridMaxPrice = MathUtil::ceilModulo(m_context.maxPrice , 4, gridRowCount);
	m_context.gridMinPrice = MathUtil::floorModulo(m_context.minPrice, 4, gridRowCount);
}

QString CandleQuoteDisplayWidget::getMinTick(const QString& symbol) {
	// Binance API 地址
	const QString apiUrl = "https://fapi.binance.com/fapi/v1/exchangeInfo";

	// 创建网络访问管理器
	QNetworkAccessManager manager;

	// 创建 HTTP 请求
	QNetworkRequest request;
	request.setUrl(QUrl(apiUrl));

	// 同步发送请求
	QNetworkReply* reply = manager.get(request);

	// 使用事件循环等待请求完成
	QEventLoop loop;
	QObject::connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
	loop.exec();

	// 检查请求是否成功
	if (reply->error() != QNetworkReply::NoError) {
		qWarning() << "Error:" << reply->errorString();
		reply->deleteLater();
		return QString();
	}

	// 解析返回的 JSON 数据
	QByteArray responseData = reply->readAll();
	reply->deleteLater();

	QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
	if (!jsonDoc.isObject()) {
		qWarning() << "Invalid JSON response";
		return QString();
	}

	QJsonObject jsonObj = jsonDoc.object();
	if (!jsonObj.contains("symbols")) {
		qWarning() << "Missing 'symbols' in response";
		return QString();
	}

	QJsonArray symbolsArray = jsonObj["symbols"].toArray();
	for (const QJsonValue& value : symbolsArray) {
		QJsonObject symbolObj = value.toObject();
		if (symbolObj["symbol"].toString() == symbol) {
			// 找到目标交易对，提取 minTick 信息
			if (symbolObj.contains("filters")) {
				QJsonArray filtersArray = symbolObj["filters"].toArray();
				for (const QJsonValue& filterValue : filtersArray) {
					QJsonObject filterObj = filterValue.toObject();
					if (filterObj["filterType"].toString() == "PRICE_FILTER") {
						return removeTrailingZeros(filterObj["tickSize"].toString());
					}
				}
			}
		}
	}

	// 如果未找到目标交易对或 minTick 信息
	qWarning() << "Symbol or minTick not found";
	return QString();
}

QString CandleQuoteDisplayWidget::removeTrailingZeros(const QString& numberStr) {
	// 使用 QRegularExpression 替代 QRegExp
	QRegularExpression trailingZerosRegex("0+$");
	QRegularExpression trailingDotRegex("\\.$");

	QString result = numberStr;
	result.remove(trailingZerosRegex); // 移除尾随的 0
	result.remove(trailingDotRegex);   // 移除孤立的小数点
	return result;
}

void CandleQuoteDisplayWidget::reset() {
	m_widthArrayIndex = 11;
	m_context.width = WIDTH_ARRAY[m_widthArrayIndex];
	m_context.spacing = 0.5 * m_context.width;

	this->m_isFirstTime = true;
}

void CandleQuoteDisplayWidget::onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList) {
	if (dataList.size() == 0) {
		return;
	}

	if (m_context.expectedSize > dataList.size()) {
		m_context.isFullData = true;
	}

	m_context.dataList.insert(m_context.dataList.begin(), dataList.begin(), dataList.end());
	if (this->m_isFirstTime) {
		this->reset();

		// 计算刻度文本长度
		double close = dataList.back()->close;
		m_context.gridScaleTextWidth = QFontMetrics(QApplication::font()).horizontalAdvance(QString::number(close));

		// 要预留刻度的空间
		int candleCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, width() - this->m_context.gridScaleAreaWidth());

		this->m_context.endIndex = (int)m_context.dataList.size();
		this->m_context.startIndex = this->m_context.endIndex - candleCount;

	}
	else {
		this->m_context.startIndex = 0;
		if (m_context.isFullData) {
			m_context.endIndex = std::min((int)m_context.dataList.size(), m_context.endIndex + (m_context.expectedSize - (int)dataList.size()));
		}
		else {
			m_context.endIndex = std::min((int)m_context.dataList.size(), m_context.endIndex + (int)dataList.size());
		}
	}

	m_context.expectedSize = 0;
	this->m_isFirstTime = false;

 	this->recalculateContextParam();
	this->m_candleChart->update();
	this->m_volumeChart->update();
}

void CandleQuoteDisplayWidget::repaintChart() {
	this->m_candleChart->repaint();
	this->m_volumeChart->repaint();
}