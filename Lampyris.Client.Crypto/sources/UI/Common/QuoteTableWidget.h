// Project Include(s)
#include <QWidget>
#include <QVBoxLayout>
#include <QTimer>
#include <QNetworkAccessManager>
#include <QNetworkReply>

// STD Include(s)
#include <vector>

// Project Include(s)
#include "QuoteListItem.h"
#include "TableHeader.h"
#include "AppSystem/Quote/Const/TickerDataSortType.h"
#include "AppSystem/Quote/Data/QuoteTickerDataView.h"
#include "Const/UIDisplayMode.h"
#include "UI/Common/CrossPlatformWidget.h"

enum class QuoteTableListMode {
    QUOTE_LIST = 0,
    SEARCH_LIST = 1,
};

class QuoteTableWidget : public CrossPlatformWidget {
    Q_OBJECT
    
    const QString COLUMN_NAME_SYMBOL       = "名称";
    const QString COLUMN_NAME_PRICE        = "价格";
    const QString COLUMN_NAME_24H_CURRENCY = "24h成交额";
    const QString COLUMN_NAME_RISE_SPEED   = "涨速";
    const QString COLUMN_NAME_PERCENTAGE   = "涨跌幅";

    std::unordered_map<QString, TickerDataSortType> m_columnName2SortType = {
        {COLUMN_NAME_SYMBOL,       TickerDataSortType::NAME},
        {COLUMN_NAME_PRICE,        TickerDataSortType::PRICE},
        {COLUMN_NAME_24H_CURRENCY, TickerDataSortType::CURRENCY},
        {COLUMN_NAME_RISE_SPEED,   TickerDataSortType::RISE_SPEED},
        {COLUMN_NAME_PERCENTAGE,   TickerDataSortType::PERCENTAGE},
    };
public:
    explicit                    QuoteTableWidget(UIDisplayMode displayMode, QuoteTableListMode listMode, QWidget* parent = nullptr);
    virtual~                    QuoteTableWidget();
protected:
    virtual void                setupDisplayMode(UIDisplayMode displayMode);
private slots:
    void                        onColumnWidthResized(const std::vector<TableColumnWidthInfo>& fieldWidths);
    void                        onSortRequested(const QString& fieldName, DataSortingOrder sortOrder);
private:
    TableHeader*                m_tableHeader;
    QVBoxLayout*                m_layout;
    std::vector<QuoteListItem*> m_items;
    QString                     m_sortField;
    DataSortingOrder            m_sortOrder;
    QuoteTickerDataViewPtr      m_dataView;
    int                         m_updateCallbackId;
    QuoteTableListMode          m_listMode;
    QuoteListItemDisplayMode    m_itemMode;

    void                        sortItems();
    void                        handleDataUpdate();
};
