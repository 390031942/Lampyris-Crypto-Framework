// Project Include(s)
#include "QuoteCandleDataView.h"

QuoteCandleDataView::QuoteCandleDataView(const QString& symbol, BarSize barSize)
    : m_displaySize(m_displaySize), m_startIndex(0), m_focusIndex(-1), m_lastSegmentSize(0) {
    QObject::connect(&m_binanceAPI, &BinanceAPI::dataFetched, [this](const std::vector<QuoteCandleDataPtr>& dataList) {
        this->dataFetched(dataList);
    });
    m_binanceAPI.fetchKlines(symbol, "1m", QUOTE_CANDLE_DATA_SEGMENT_SIZE);
}

QuoteCandleDataView::~QuoteCandleDataView() {

}

// 向左移动视图
void QuoteCandleDataView::moveLeft() {
    m_startIndex = std::clamp(m_startIndex--, 0, m_displaySize - 1);
}

// 向右移动视图
void QuoteCandleDataView::moveRight() {
    m_startIndex = std::clamp(m_startIndex++, 0, m_displaySize - 1);
}

void QuoteCandleDataView::slideLeft(int pixel) {}

void QuoteCandleDataView::slideRight(int pixel) {}

void QuoteCandleDataView::setFocusIndex(int index) {
    m_focusIndex = std::clamp(index, 0, m_displaySize - 1);
}

void QuoteCandleDataView::expand() {
    if (m_isLoading) {
        return;
    }

    int totalSize = getTotalSize();
    int endIndex = m_displaySize + m_startIndex;
    int diff = displaySize - m_displaySize;
    int newStartIndex = endIndex - diff;

    if (newStartIndex < 0) {
        if (m_isFullData) { // 数据完备
            m_startIndex = 0;
        }
        else { // 需要下载历史数据的情况
            // 分配新的segment, 需要请求新的数据
            // 新的segment部分的数据在下载完成之前，k线图里会显示为空白部分。
            m_isLoading = true;
            m_isFullData = QuoteManager::getInstance()->requestCandleDataForView(this);
            m_startIndex = newStartIndex;
        }
    }
}

void QuoteCandleDataView::shrink() {
    int totalSize = getTotalSize();
    int endIndex = m_displaySize + m_startIndex;
    int diff = displaySize - m_displaySize;
    int newStartIndex = endIndex + diff;

    if (m_focusIndex >= 0) { // 如果有当前聚焦的k线,则需要需要逐渐让聚焦的k线显示在中间
        // 聚焦的k线在水平坐标轴上的比例
        double focusRatio = m_focusIndex / m_displaySize;

        // 调整前:
        // |      左侧k线      |focusIndex|      右侧k线      |
        // 
        // 调整后(左右侧的k线数目减少，且聚焦的k线不变):
        //         |  左侧k线  |focusIndex|  右侧k线 |
        // 
        // 为了实现这个效果，首先计算左侧和右侧k线需要减少的数目
        int leftReduceCount = diff * focusRatio;
        int rightReduceCount = diff - leftReduceCount;

        // 只需要让startIndex往右偏移leftReduceCount即可，同时focusIndex需要减去偏移
        m_startIndex = m_startIndex + leftReduceCount;
        m_focusIndex = m_focusIndex - leftReduceCount;

        // 最后实现聚焦k线往中间靠拢的效果
        if ((focusRatio > 0.5 && focusRatio < 0.75f) || (focusRatio < 0.5 && focusRatio > 0.25f)) {
            focusRatio = 0.5;
        }
        else if (focusRatio > 0.75f) {
            focusRatio -= 0.25f;
        }
        else if (focusRatio < 0.25f) {
            focusRatio += 0.25f;
        }
    }

    m_startIndex = newStartIndex;
}

// 获取视图中展示的 K 线数量
int QuoteCandleDataView::getDisplaySize() const {
    return m_displaySize;
}

// 获取完整数据的总大小
size_t QuoteCandleDataView::getTotalSize() const {
    return (m_segments.size() - 1) * QUOTE_CANDLE_DATA_SEGMENT_SIZE + m_lastSegmentSize;
}

// 根据全局索引获取 K 线数据
const QuoteCandleDataPtr& QuoteCandleDataView::getCandleDataByGlobalIndex(int globalIndex) const {
    if (globalIndex <= 0 && globalIndex >= getTotalSize()) {
        return nullptr;  // 越界返回默认值
    }

    int segmentIndex = globalIndex / QUOTE_CANDLE_DATA_SEGMENT_SIZE;
    int inSegmentIndex = globalIndex - (m_segments.size() - 1) * QUOTE_CANDLE_DATA_SEGMENT_SIZE;
    return (*m_segments[segmentIndex])[inSegmentIndex];
}

// 运算符[]：根据视图中的索引返回 K 线数据
const QuoteCandleDataPtr& QuoteCandleDataView::operator[](int index) const {
    if (index <= 0 || index >= m_displaySize) {
        return nullptr;  // 越界返回默认值
    }
    int globalIndex = m_startIndex + index;
    return getCandleDataByGlobalIndex(globalIndex);
}

void QuoteCandleDataView::notifyDataReceived() {
    m_isLoading = false;
}

// 获取最后一个数据对应的DateTime
QDateTime QuoteCandleDataView::getFirstDataDateTime() {
    const auto& quoteCandleData = getCandleDataByGlobalIndex(0);
    return quoteCandleData->dateTime;
}

void QuoteCandleDataView::dataFetched(const std::vector<QuoteCandleDataPtr>& dataList) {
    if (dataList.empty())
        return;

    bool isFirstReceived = m_segments.size() == 0;
    if (isFirstReceived) {
        m_segments.push_back(std::make_shared<QuoteCandleDataSegment>());
    }

    int currentIndex = 0;

    // 将数据塞入分段中
    while (true) {
        auto& segment = *m_segments.back();
        int canAddCount = std::min(dataList.size() - currentIndex, QUOTE_CANDLE_DATA_SEGMENT_SIZE - segment.size());
        int startAddIndex = segment.size();
        for (int i = startAddIndex; i < startAddIndex + canAddCount; i++) {
            segment[i] = dataList[currentIndex];
            currentIndex++;
        }

        // 数据加完了，跳出
        if (currentIndex == dataList.size() - 1) {
            break;
        }
        
        // 插入新的数据段
        m_segments.push_back(std::make_shared<QuoteCandleDataSegment>());
    }
}
