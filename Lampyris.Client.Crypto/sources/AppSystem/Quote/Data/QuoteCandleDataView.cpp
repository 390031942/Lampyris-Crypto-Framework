// Project Include(s)
#include "QuoteCandleDataView.h"
#include "AppSystem/Quote/Manager/QuoteManager.h"

QuoteCandleDataView::QuoteCandleDataView(size_t m_displaySize)
    : m_displaySize(m_displaySize), m_startIndex(0), m_focusIndex(-1), m_lastSegmentSize(0) {
    m_dynamicSegment = QuoteManager::getInstance()->allocateDynamicSegment();
}

QuoteCandleDataView::~QuoteCandleDataView() {
    QuoteManager::getInstance()->recycleDynamicSegement(m_dynamicSegment);
    for (auto segment : m_segments) {
        QuoteManager::getInstance()->recycleSegement(segment);
    }
}

// �����ƶ���ͼ
void QuoteCandleDataView::moveLeft() {
    m_startIndex = std::clamp(m_startIndex--, 0, m_displaySize - 1);
}

// �����ƶ���ͼ
void QuoteCandleDataView::moveRight() {
    m_startIndex = std::clamp(m_startIndex++, 0, m_displaySize - 1);
}

void QuoteCandleDataView::setFocusIndex(int index) {
    m_focusIndex = std::clamp(index, 0, m_displaySize - 1);
}

void QuoteCandleDataView::expand(int displaySize) {
    if (displaySize <= m_displaySize) {
        return;
    }

    if (m_isLoading) {
        return;
    }

    int totalSize = getTotalSize();
    int endIndex = m_displaySize + m_startIndex;
    int diff = displaySize - m_displaySize;
    int newStartIndex = endIndex - diff;

    if (newStartIndex < 0) {
        if (m_isFullData) { // �����걸
            m_startIndex = 0;
        }
        else { // ��Ҫ������ʷ���ݵ����
            // �����µ�segment, ��Ҫ�����µ�����
            // �µ�segment���ֵ��������������֮ǰ��k��ͼ�����ʾΪ�հײ��֡�
            m_isLoading = true;
            m_isFullData = QuoteManager::getInstance()->requestCandleDataForView(this);
            m_startIndex = newStartIndex;
        }
    }
}

void QuoteCandleDataView::shrink(int displaySize) {
    if (displaySize >= m_displaySize) {
        return;
    }

    int totalSize = getTotalSize();
    int endIndex = m_displaySize + m_startIndex;
    int diff = displaySize - m_displaySize;
    int newStartIndex = endIndex + diff;

    if (m_focusIndex >= 0) { // ����е�ǰ�۽���k��,����Ҫ��Ҫ���þ۽���k����ʾ���м�
        // �۽���k����ˮƽ�������ϵı���
        double focusRatio = m_focusIndex / m_displaySize;

        // ����ǰ:
        // |      ���k��      |focusIndex|      �Ҳ�k��      |
        // 
        // ������(���Ҳ��k����Ŀ���٣��Ҿ۽���k�߲���):
        //         |  ���k��  |focusIndex|  �Ҳ�k�� |
        // 
        // Ϊ��ʵ�����Ч�������ȼ��������Ҳ�k����Ҫ���ٵ���Ŀ
        int leftReduceCount = diff * focusRatio;
        int rightReduceCount = diff - leftReduceCount;

        // ֻ��Ҫ��startIndex����ƫ��leftReduceCount���ɣ�ͬʱfocusIndex��Ҫ��ȥƫ��
        m_startIndex = m_startIndex + leftReduceCount;
        m_focusIndex = m_focusIndex - leftReduceCount;

        // ���ʵ�־۽�k�����м俿£��Ч��
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

// ��ȡ��ͼ��չʾ�� K ������
int QuoteCandleDataView::getDisplaySize() const {
    return m_displaySize;
}

// ��ȡ�������ݵ��ܴ�С
size_t QuoteCandleDataView::getTotalSize() const {
    return (m_segments.size() - 1) * QUOTE_CANDLE_DATA_SEGMENT_SIZE + m_lastSegmentSize;
}

// ����ȫ��������ȡ K ������
const QuoteCandleData& QuoteCandleDataView::getCandleDataByGlobalIndex(int globalIndex) const {
    if (globalIndex <= 0 && globalIndex >= getTotalSize()) {
        return QuoteCandleData();  // Խ�緵��Ĭ��ֵ
    }

    int segmentIndex = globalIndex / QUOTE_CANDLE_DATA_SEGMENT_SIZE;
    int inSegmentIndex = globalIndex - (m_segments.size() - 1) * QUOTE_CANDLE_DATA_SEGMENT_SIZE;
    return (*m_segments[segmentIndex])[inSegmentIndex];
}

// �����[]��������ͼ�е��������� K ������
const QuoteCandleData& QuoteCandleDataView::operator[](int index) const {
    if (index <= 0 || index >= m_displaySize) {
        return QuoteCandleData();  // Խ�緵��Ĭ��ֵ
    }
    int globalIndex = m_startIndex + index;
    return getCandleDataByGlobalIndex(globalIndex);
}

void QuoteCandleDataView::notifyDataReceived() {
    m_isLoading = false;
}

// ��ȡ���һ�����ݶ�Ӧ��DateTime
QDateTime QuoteCandleDataView::getFirstDataDateTime() {
    const auto& quoteCandleData = getCandleDataByGlobalIndex(0);
    return quoteCandleData.dateTime;
}
