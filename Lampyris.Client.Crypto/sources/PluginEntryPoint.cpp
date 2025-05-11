// Project Include(s)
#include "PluginEntryPoint.h"
#include "UI/Standalong/Quote/QuoteTickerDataWidget.h"
#include "UI/Common/AssetTrendCurveWidget.h"
#include "UI/Common/MarketOverviewBarChartWidget.h"

#include <QFile>

#if defined(LAMPYRIS_EXE)
int main1(int argc, char** argv) {
    QApplication a(argc, argv);
    // 加载资源文件中的 main.qss
    QFile qssFile(":/res/qss/main.qss");
    if (qssFile.open(QFile::ReadOnly)) {
        QString qss = qssFile.readAll();
        a.setStyleSheet(qss); // 设置应用程序样式表
        qssFile.close();
    }
    else {
        qDebug() << "Failed to load QSS file.";
    }

    AssetTrendCurveWidget mainWidget;
    mainWidget.show();
    return a.exec();
}

int main3(int argc, char* argv[]) {
    QApplication app(argc, argv);

    MarketOverviewBarChartWidget widget;

    // 随机生成测试数据
    QVector<MarketPreviewIntervalDataBean> data = {
        {-1, -7, 5},  // <-7%
        {-7, -5, 10}, // -7%~-5%
        {-5, -3, 15}, // -5%~-3%
        {-3, -1, 20}, // -3%~-1%
        {0, 0, 25},   // 平盘
        {1, 3, 30},   // 1%~3%
        {3, 5, 20},   // 3%~5%
        {5, 7, 15},   // 5%~7%
        {7, -1, 10},  // >7%
    };
    widget.setData(data);
    widget.setBarWidth(40); // 设置柱状宽度
    widget.resize(800, 600);
    widget.show();

    return app.exec();
}

#include "UI/Mobile/Common/BottomPopupWidget.h"
int main(int argc, char* argv[]) {
    QApplication app(argc, argv);

    // 主窗口
    QWidget mainWindow;
    mainWindow.setWindowTitle("Bottom Popup Example");
    mainWindow.resize(400, 800); // 模拟移动端竖屏分辨率
    mainWindow.setGeometry(720, 100, 400, 800);
    mainWindow.setWindowFlag(Qt::FramelessWindowHint);
    QVBoxLayout* layout = new QVBoxLayout(&mainWindow);

    QPushButton* openPopupButton = new QPushButton("打开弹出窗口");
    layout->addWidget(openPopupButton);

    // 创建底部弹出窗口
    BottomPopupWidget* popupWidget = new BottomPopupWidget(&mainWindow);

    // 设置弹出窗口内容
    QWidget* contentWidget = new QWidget();
    QVBoxLayout* contentLayout = new QVBoxLayout(contentWidget);
    contentLayout->addWidget(new QLabel("这是弹出窗口的内容"));
    contentLayout->addWidget(new QPushButton("按钮1"));
    contentLayout->addWidget(new QPushButton("按钮2"));
    popupWidget->setContentWidget(contentWidget);

    // 点击按钮时显示弹出窗口
    QObject::connect(openPopupButton, &QPushButton::clicked, [&]() {
        // popupWidget->showPopup();
    });

    mainWindow.show();
    return app.exec();
}

#endif // !LAMPYRIS_EXE