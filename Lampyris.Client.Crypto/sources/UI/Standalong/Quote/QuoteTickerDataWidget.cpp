#include "QuoteTickerDataWidget.h"
#include <QScrollBar>
#include <QDebug>
#include <QPixmap>

#include "UI/Common/GlobalUIStyle.h"

QuoteTickerDataWidget::QuoteTickerDataWidget(QWidget* parent)
    : QWidget(parent), sortedColumn(-1), ascendingOrder(true) {
    // 设置布局
    QVBoxLayout* layout = new QVBoxLayout(this);
    // 创建表格控件
    tableWidget = new QTableWidget(this);
    tableWidget->setColumnCount(11); // 设置列数为 11
    tableWidget->setHorizontalScrollBarPolicy(Qt::ScrollBarAsNeeded); // 水平滚动条按需显示
    tableWidget->setVerticalScrollBarPolicy(Qt::ScrollBarAsNeeded);   // 垂直滚动条按需显示
    tableWidget->horizontalHeader()->setSectionResizeMode(QHeaderView::Stretch); // 表头自动调整宽度
    tableWidget->setSelectionBehavior(QAbstractItemView::SelectRows); // 选择整行
    tableWidget->setSelectionMode(QAbstractItemView::SingleSelection); // 单选模式

    // 设置表头为中文
    QStringList headers = { "序号", "交易对", "价格", "涨跌幅", "24h成交额", "24h成交量", "开盘价", "最高价", "最低价", "上一笔成交量", "成交笔数" };
    tableWidget->setHorizontalHeaderLabels(headers);

    // 设置整个表格控件的样式
    tableWidget->setStyleSheet(
        "QTableWidget {"
        "    background-color: black;" // 表格背景色
        "    color: white;"            // 表格文字颜色
        "    gridline-color: gray;"    // 表格网格线颜色
        "}"
        "QHeaderView::section {"
        "    background-color: black;" // 表头背景色
        "    color: white;"            // 表头文字颜色
        "    border: 1px solid gray;"  // 表头边框
        "}"
        "QTableCornerButton::section {"
        "    background-color: black;" // 左上角序号背景色
        "    border: 1px solid gray;"  // 左上角边框
        "}"
        "QScrollBar:vertical, QScrollBar:horizontal {"
        "    background-color: black;" // 滚动条背景色
        "}"
    );

    // 初始化 QTableWidgetItem 池
    int initialRows = 100; // 假设初始池大小为 100 行
    itemPool.resize(initialRows, std::vector<QTableWidgetItem*>(11, nullptr));
    for (int i = 0; i < initialRows; ++i) {
        for (int j = 0; j < 11; ++j) {
            itemPool[i][j] = new QTableWidgetItem();
        }
    }

    // 连接表头点击事件
    connect(tableWidget->horizontalHeader(), &QHeaderView::sectionClicked, this, &QuoteTickerDataWidget::onHeaderClicked);

    layout->addWidget(tableWidget);

    // 创建网络管理器
    networkManager = new QNetworkAccessManager(this);
    connect(networkManager, &QNetworkAccessManager::finished, this, &QuoteTickerDataWidget::onReplyFinished);

    // 创建定时器
    updateTimer = new QTimer(this);
    connect(updateTimer, &QTimer::timeout, this, &QuoteTickerDataWidget::fetchTickerData);
    updateTimer->start(1500); // 每 5 秒更新一次数据

    // 获取数据
    fetchTickerData();
}

void QuoteTickerDataWidget::fetchTickerData() {
    // Binance 永续合约 API URL
    QString url = "https://fapi.binance.com/fapi/v1/ticker/24hr";

    // 创建 QNetworkRequest 对象
    QNetworkRequest request;
    request.setUrl(QUrl(url)); // 使用 setUrl 设置请求的 URL

    // 发起 GET 请求
    networkManager->get(request);
}

void QuoteTickerDataWidget::onReplyFinished(QNetworkReply* reply) {
    if (reply->error() != QNetworkReply::NoError) {
        qDebug() << "Error fetching data:" << reply->errorString();
        reply->deleteLater();
        return;
    }

    // 解析 JSON 数据
    QByteArray responseData = reply->readAll();
    QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
    if (!jsonDoc.isArray()) {
        qDebug() << "Invalid JSON format";
        reply->deleteLater();
        return;
    }

    QJsonArray jsonArray = jsonDoc.array();

    // 如果行数超过池大小，扩展池
    if (jsonArray.size() > itemPool.size()) {
        int currentSize = itemPool.size();
        itemPool.resize(jsonArray.size(), std::vector<QTableWidgetItem*>(11, nullptr));
        for (int i = currentSize; i < jsonArray.size(); ++i) {
            for (int j = 0; j < 11; ++j) {
                itemPool[i][j] = new QTableWidgetItem();
            }
        }
    }

    // 填充数据
    tableWidget->setRowCount(jsonArray.size());
    // 遍历 JSON 数据，更新表格中的数据
    for (const QJsonValue& value : jsonArray) {
        QJsonObject ticker = value.toObject();
        QString symbol = ticker["symbol"].toString();
        double currentPrice = ticker["lastPrice"].toString().toDouble();
        double priceChangePercent = ticker["priceChangePercent"].toString().toDouble();

        // 查找 symbol 对应的行号
        int i = -1;
        if (symbolToRowMap.contains(symbol)) {
            i = symbolToRowMap[symbol]; // 获取表格中该 symbol 的行号
        }
        else {
            // 如果表格中不存在该 symbol，则添加到表格末尾
            i = symbolToRowMap.size();
            tableWidget->insertRow(i);
            symbolToRowMap[symbol] = i; // 更新映射
        }

        // 设置序号
        QTableWidgetItem* item0 = itemPool[i][0];
        item0->setText(QString::number(i + 1));
        tableWidget->setItem(i, 0, item0);

        // 设置交易对
        QTableWidgetItem* item1 = itemPool[i][1];
        item1->setText(symbol);
        tableWidget->setItem(i, 1, item1);

        // 设置价格
        QTableWidgetItem* item2 = itemPool[i][2];
        item2->setText(QString::number(currentPrice, 'f', 2));
        item2->setForeground(priceChangePercent > 0 ? GlobalUIStyle::green : GlobalUIStyle::red); // 根据涨跌幅设置颜色
        tableWidget->setItem(i, 2, item2);

        // 设置涨跌幅
        QTableWidgetItem* item3 = itemPool[i][3];
        item3->setText(QString::number(priceChangePercent, 'f', 2) + "%");
        item3->setForeground(priceChangePercent > 0 ? GlobalUIStyle::green : GlobalUIStyle::red); // 根据涨跌幅设置颜色
        tableWidget->setItem(i, 3, item3);

        // 设置24h成交额
        QTableWidgetItem* item4 = itemPool[i][4];
        item4->setText(ticker["quoteVolume"].toString());
        tableWidget->setItem(i, 4, item4);

        // 设置24h成交量
        QTableWidgetItem* item5 = itemPool[i][5];
        item5->setText(ticker["volume"].toString());
        tableWidget->setItem(i, 5, item5);

        // 设置开盘价
        QTableWidgetItem* item6 = itemPool[i][6];
        item6->setText(ticker["openPrice"].toString());
        tableWidget->setItem(i, 6, item6);

        // 设置最高价
        QTableWidgetItem* item7 = itemPool[i][7];
        item7->setText(ticker["highPrice"].toString());
        tableWidget->setItem(i, 7, item7);

        // 设置最低价
        QTableWidgetItem* item8 = itemPool[i][8];
        item8->setText(ticker["lowPrice"].toString());
        tableWidget->setItem(i, 8, item8);

        // 设置上一笔成交量
        QTableWidgetItem* item9 = itemPool[i][9];
        item9->setText(ticker["lastQty"].toString());
        tableWidget->setItem(i, 9, item9);

        // 设置成交笔数
        QTableWidgetItem* item10 = itemPool[i][10];
        item10->setText(ticker["count"].toString());
        tableWidget->setItem(i, 10, item10);

        // 更新 lastPrices
        lastPrices[symbol] = currentPrice;
    }

    reply->deleteLater();
}

void QuoteTickerDataWidget::onHeaderClicked(int index) {
    // 排序表格数据
    tableWidget->sortItems(index, ascendingOrder ? Qt::AscendingOrder : Qt::DescendingOrder);
    ascendingOrder = !ascendingOrder; // 切换排序顺序
}

void QuoteTickerDataWidget::resetCellColor(int row, int column) {
    QTableWidgetItem* item = tableWidget->item(row, column);
    if (item) {
        item->setBackground(Qt::black); // 恢复背景色为黑色
    }
}
