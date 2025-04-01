#include "TestWidget.h"
#include "MathUtil.h"
#include "PlotRenderer.h"
#include <qlayout.h>
#include <QTimer>

// �¼�������
bool TestWidget::eventFilter(QObject* watched, QEvent* event) {
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
	// Ĭ�ϴ���
	return QWidget::eventFilter(watched, event);
}

void TestWidget::resizeEvent(QResizeEvent* e) {
	if (!m_context.dataList.empty()) {
		QSize size = e->size();
		int width = size.width();

		int oldCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, e->oldSize().width());
		int newCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, e->size().width());

		if (oldCount != newCount && oldCount < m_context.dataList.size()) {
			m_context.startIndex = m_context.endIndex - newCount;
		}
		recalculateContextParam();
		this->repaintChart();
	}
	QWidget::resizeEvent(e);
}

void TestWidget::keyPressEvent(QKeyEvent* e) {
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

void TestWidget::mouseMoveEvent(QMouseEvent* e) {
	this->handleMouseMove(e);
	QWidget::mouseMoveEvent(e);
}

void TestWidget::handleMouseMove(QMouseEvent* e) {
	QPoint pos = e->pos();
	int x = pos.x();
	int focusIndex = m_context.focusIndex;

	// ����focuxsIndex
	m_context.focusIndex = calculateFocusIndex(x, m_context.leftOffset, m_context.width, m_context.spacing, m_context.endIndex - m_context.startIndex) + m_context.startIndex;
	if (focusIndex != m_context.focusIndex) {
		qDebug() << m_context.focusIndex;
	}

	this->repaintChart();
}

int TestWidget::calculateFocusIndex(double X, double leftOffset, double width, double spacing, int numKlines) {
	// ÿ�� K �ߵ��ܿ��
	double totalWidth = width + spacing;

	// �������� K �������ƫ����
	double relativeX = X - leftOffset;

	// �������� K ����������
	if (relativeX < 0) {
		return 0;
	}

	// �������� K ��������Ҳ�
	if (relativeX > numKlines * totalWidth) {
		return numKlines - 1;
	}

	// ���µ� K ������
	int approxIndex = floor(relativeX / totalWidth);

	// ��� K �ߵ�����λ��
	double centerLeft = leftOffset + approxIndex * totalWidth + width / 2;

	// �Ҳ� K �ߵ�����λ��
	double centerRight = centerLeft + totalWidth;

	// �ж�������ĸ� K �ߵ����ĸ���
	int focusIndex;
	if (fabs(X - centerLeft) <= fabs(X - centerRight)) {
		focusIndex = approxIndex;
	}
	else {
		focusIndex = approxIndex + 1;
	}

	// ȷ�������Ϸ�
	if (focusIndex < 0) {
		focusIndex = 0;
	}
	else if (focusIndex >= numKlines) {
		focusIndex = numKlines - 1;
	}

	return focusIndex;
}

int TestWidget::calculateCandleCount(double leftOffset, double width, double spacing, double windowWidth) {
	// ÿ�� K �ߵ��ܿ��
	double totalWidth = width + spacing;

	// ���û��ƿ��
	double availableWidth = windowWidth - leftOffset;

	// ������ÿ�Ȳ�������ʾ�κ� K ��
	if (availableWidth <= 0) {
		return 0;
	}

	// ���������ʾ�� K ������
	int candleCount = floor(availableWidth / totalWidth);

	return candleCount;
}

TestWidget::TestWidget(QWidget *parent)
	: QWidget(parent), 
	m_candleChart(new CandleChartWidget(this)), 
	m_volumeChart(new VolumeChartWidget(this)),
	m_betterSplitter(new BetterSplitter(Qt::Orientation::Vertical,this)) {

	// ���� QSplitter �� handle ��ʽ
	m_betterSplitter->setStyleSheet("QSplitter::handle { background: gray; }");
	m_betterSplitter->setHandleWidth(1);

	QHBoxLayout* layout = new QHBoxLayout(this);
	layout->setContentsMargins(0, 0, 0, 0);
	this->setLayout(layout);
	layout->addWidget(m_betterSplitter);
	layout->addWidget(m_candleChart);

	m_betterSplitter->addWidget(m_candleChart);
	m_betterSplitter->addWidget(m_volumeChart);

	QObject::connect(&api, &BinanceAPI::dataFetched, this, &TestWidget::onDataFetched);
	api.fetchKlines("VINEUSDT", "1m", 1000);
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

	// timer->setInterval(30);
	// timer->start();
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
			if (m_context.focusIndex < m_context.startIndex) {
				m_context.startIndex -= 1;
				m_context.endIndex -= 1;
			}
		}

		if (m_context.focusIndex < 0) {
			if (!m_context.isFullData) {
				// �����µ���ʷ����
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("VINEUSDT", "1m", 100, m_context.dataList.front()->dateTime);
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
		m_context.spacing = 0.5 * m_context.width;
		int expectedCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, width());
		int currentCount = m_context.endIndex - m_context.startIndex;
		int diffCount = expectedCount - currentCount;
		
		// ���¼���startIndex��endIndex
		if (m_context.focusIndex == -1) {
			m_context.startIndex -= diffCount;

			if (m_context.startIndex < 0) {
				// �����µ���ʷ����
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("VINEUSDT", "1m", -m_context.startIndex, m_context.dataList.front()->dateTime);
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
				// �����µ���ʷ����
				m_context.isWaitingHistoryData = true;
				api.fetchKlinesFromEndTime("VINEUSDT", "1m", -m_context.startIndex, m_context.dataList.front()->dateTime);
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

void TestWidget::recalculateContextParam() {
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
	 
	// ����grid��߼ۺ���ͼ�
	const int gridRowCount = 5; 
	const int gridColCount = 4;

 	m_context.gridMaxPrice = MathUtil::ceilModulo(m_context.maxPrice , 4, gridRowCount);
	m_context.gridMinPrice = MathUtil::floorModulo(m_context.minPrice, 4, gridRowCount);
}

void TestWidget::reset() {
	m_widthArrayIndex = 11;
	m_context.width = widthArray[m_widthArrayIndex];
	m_context.spacing = 0.5 * m_context.width;

	this->m_isFirstTime = true;
}

void TestWidget::onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList) {
	if (dataList.size() == 0) {
		return;
	}

	if (m_context.expectedSize > dataList.size()) {
		m_context.isFullData = true;
	}

	m_context.dataList.insert(m_context.dataList.begin(), dataList.begin(), dataList.end());
	if (this->m_isFirstTime) {
		this->reset();

		// ����̶��ı�����
		m_context.gridScaleTextWidth = QFontMetrics(QApplication::font()).horizontalAdvance(QString::number(dataList.back()->close));

		// ҪԤ���̶ȵĿռ�
		int scaleTextTotalWidth = m_context.gridScaleTextWidth + m_context.gridScaleTextLeftPadding + m_context.gridScaleTextRightPadding;
		int candleCount = this->calculateCandleCount(m_context.leftOffset, m_context.width, m_context.spacing, width() - scaleTextTotalWidth);

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
