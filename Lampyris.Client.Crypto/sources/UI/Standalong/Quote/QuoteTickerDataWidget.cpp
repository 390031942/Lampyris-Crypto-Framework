#include "QuoteTickerDataWidget.h"
#include <QScrollBar>
#include <QDebug>
#include <QPixmap>
#include "UI/Common/GlobalUIStyle.h"
#include <QPushButton>

QuoteTickerDataWidget::QuoteTickerDataWidget(QWidget* parent)
    : QWidget(parent), sortedColumn(-1), ascendingOrder(true) {
    // 设置布局
    QVBoxLayout* layout = new QVBoxLayout(this);
    layout->setContentsMargins(0, 0, 0, 0);

    // 创建表格控件
    tableWidget = new QTableWidget(this);
    tableWidget->setColumnCount(6);
    tableWidget->setHorizontalScrollBarPolicy(Qt::ScrollBarAsNeeded); // 水平滚动条按需显示
    tableWidget->setVerticalScrollBarPolicy(Qt::ScrollBarAsNeeded);   // 垂直滚动条按需显示
    tableWidget->horizontalHeader()->setSectionResizeMode(QHeaderView::Stretch); // 表头自动调整宽度
    tableWidget->setSelectionBehavior(QAbstractItemView::SelectRows); // 选择整行
    tableWidget->setSelectionMode(QAbstractItemView::NoSelection);    // 禁用选中效果
    tableWidget->setEditTriggers(QAbstractItemView::EditTrigger::NoEditTriggers); 
    tableWidget->setGridStyle(Qt::NoPen);                             // 去掉表格线
    tableWidget->setStyleSheet("QTableWidget::item:selected { background-color: transparent; }"); // 禁用选中行背景色

    // 隐藏垂直表头
    tableWidget->verticalHeader()->setVisible(false);

    // 设置表头为中文
    QStringList headers = { "交易对", "价格", "涨跌幅", "24h成交额", "涨速", "展开/隐藏" };
    tableWidget->setHorizontalHeaderLabels(headers);

    // 初始化 QTableWidgetItem 池
    int initialRows = 100; // 假设初始池大小为 100 行
    itemPool.resize(initialRows, std::vector<QTableWidgetItem*>(5, nullptr));
    for (int i = 0; i < initialRows; ++i) {
        for (int j = 0; j < 5; ++j) {
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
    updateTimer->start(1500); // 每 1.5 秒更新一次数据

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

    updateTimer->stop();
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
        itemPool.resize(jsonArray.size(), std::vector<QTableWidgetItem*>(5, nullptr));
        for (int i = currentSize; i < jsonArray.size(); ++i) {
            for (int j = 0; j < 5; ++j) {
                itemPool[i][j] = new QTableWidgetItem();
            }
        }
    }

    // 填充数据
    tableWidget->setRowCount(jsonArray.size());
    for (int i = 0; i < jsonArray.size(); ++i) {
        QJsonObject ticker = jsonArray[i].toObject();
        QString symbol = ticker["symbol"].toString();
        double currentPrice = ticker["lastPrice"].toString().toDouble();
        double priceChangePercent = ticker["priceChangePercent"].toString().toDouble();

        // 设置交易对
        QTableWidgetItem* item1 = itemPool[i][0];
        item1->setText(symbol);
        tableWidget->setItem(i, 0, item1);

        // 设置价格
        QTableWidgetItem* item2 = itemPool[i][1];
        item2->setText(QString::number(currentPrice, 'f', 2));
        item2->setForeground(priceChangePercent > 0 ? GlobalUIStyle::green : GlobalUIStyle::red); // 根据涨跌幅设置颜色
        tableWidget->setItem(i, 1, item2);

        // 设置涨跌幅
        QTableWidgetItem* item3 = itemPool[i][2];
        item3->setText(QString::number(priceChangePercent, 'f', 2) + "%");
        item3->setForeground(priceChangePercent > 0 ? GlobalUIStyle::green : GlobalUIStyle::red); // 根据涨跌幅设置颜色
        tableWidget->setItem(i, 2, item3);

        // 设置24h成交额
        QTableWidgetItem* item4 = itemPool[i][3];
        item4->setText(ticker["quoteVolume"].toString());
        tableWidget->setItem(i, 3, item4);

        // 设置涨速
        QTableWidgetItem* item5 = itemPool[i][4];
        item5->setText(ticker["volume"].toString());
        tableWidget->setItem(i, 4, item5);

        // 添加 "展开/隐藏" 按钮
        QPushButton* expandButton = new QPushButton("展开");
        connect(expandButton, &QPushButton::clicked, this, [this, i, expandButton]() {
            toggleExpandRow(i, expandButton);
            });
        tableWidget->setCellWidget(i, 5, expandButton);

        // 更新 lastPrices
        lastPrices[symbol] = currentPrice;
    }

    reply->deleteLater();
}

void QuoteTickerDataWidget::toggleExpandRow(int row, QPushButton* button) {
    static QSet<int> expandedRows; // 记录已展开的行

    if (expandedRows.contains(row)) {
        // 如果已经展开，则隐藏
        tableWidget->removeRow(row + 1);
        expandedRows.remove(row);
        button->setText("展开");
    }
    else {
        // 如果未展开，则显示
        tableWidget->insertRow(row + 1);

        // 创建一个自定义窗口
        QWidget* expandWidget = new QWidget();
        QVBoxLayout* layout = new QVBoxLayout(expandWidget);
        layout->setContentsMargins(0, 0, 0, 0);
        layout->addWidget(new QLabel(QString("详细信息：行 %1").arg(row)));
        expandWidget->setLayout(layout);

        // 跨列显示
        tableWidget->setSpan(row + 1, 0, 1, 6);
        tableWidget->setCellWidget(row + 1, 0, expandWidget);

        expandedRows.insert(row);
        button->setText("隐藏");
    }
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