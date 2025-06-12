#include "TradeWidget.h"

void TradeWidget::setupLayout() {
	LayoutBuilder builder(this);
	QFont font = QApplication::font();
	font.setPointSizeF(8);

	builder.beginHorizontalLayout();
	{
		builder.addLabel("可用");
		QLabel* textAvailable = builder.addLabel();
		textAvailable->setText("234.34 USDT");
		QPushButton* buttonAssetExchange = builder.addIconButton(":/res/icons/asset_exchange.png",QSize(16,16));
		buttonAssetExchange->setCursor(Qt::PointingHandCursor);
		builder.addSpacerItem();
	}
	builder.endLayout();

	builder.beginHorizontalLayout();
	{
		QLabel* label = builder.addLabel("价格");
		label->setFont(font);
		builder.addSpacerItem();
	}
	builder.endLayout();

	BetterLineEdit* textPrice = builder.addLineEdit();
	textPrice->setValidator(new QDoubleValidator(textPrice));
	textPrice->setOptions({ "亏损比例(%)" ,"指定价格", "亏损金额(USDT)" });

	builder.beginHorizontalLayout();
	{
		builder.addLabel("数量");
		builder.addSpacerItem();
	}
	builder.endLayout();

	builder.beginHorizontalLayout();
	{
		QLineEdit* textQuantity = builder.addLineEdit();
		textQuantity->setValidator(new QDoubleValidator(textQuantity));
	}
	builder.endLayout();

	builder.beginHorizontalLayout();
	{
		QLabel* textBuyQuantity = builder.addLabel();
		textBuyQuantity->setText("买入:0.00USDT");
		builder.addSpacerItem();
		QLabel* textSellQuantity = builder.addLabel();
		textSellQuantity->setText("卖出:0.00USDT");
	}
	builder.endLayout();

	builder.beginHorizontalLayout();
	{
		QCheckBox* toggleStopProfitLoss = builder.addCheckBox("止盈/止损");
	}
	builder.endLayout();

	QWidget* stopProfitLossContainer = builder.beginVerticalLayout();
	{
		builder.beginHorizontalLayout();
		{
			builder.addLabel("止盈");
			builder.addSpacerItem();
		}
		builder.endLayout();

		BetterLineEdit* textMakeProfit = builder.addLineEdit();
		textMakeProfit->setOptions({ "获利比例(%)" ,"指定价格", "盈利金额(USDT)" });

		builder.beginHorizontalLayout();
		{
			builder.addLabel("止损");
			builder.addSpacerItem();
		}
		builder.endLayout();

		BetterLineEdit* textStopLoss = builder.addLineEdit();
		textMakeProfit->setOptions({ "亏损比例(%)" ,"指定价格", "亏损金额(USDT)" });
	}
	builder.endLayout();

	builder.beginHorizontalLayout();
	{
		QPushButton* buttonLongBuy = builder.addButton("买入/做多");
		QPushButton* buttonShortSell = builder.addButton("卖出/做空");

		// 设置按钮样式表
		buttonLongBuy->setStyleSheet(
			"QPushButton {"
			"    background-color: rgb(246, 70, 93);"  // 常规颜色
			"    border: none;"                        // 去掉边框
			"    border-radius: 5px;"                 // 圆角半径
			"    color: white;"                        // 字体颜色
			"    padding: 10px;"                       // 内边距
			"}"
			"QPushButton:hover {"
			"    background-color: rgb(202, 61, 80);"  // 鼠标悬停颜色
			"}"
			"QPushButton:pressed {"
			"    background-color: rgb(202, 61, 80);"  // 鼠标点击颜色
			"}"
		);

		buttonShortSell->setStyleSheet(
			"QPushButton {"
			"    background-color: rgb(46, 189, 133);" // 常规颜色
			"    border: none;"                        // 去掉边框
			"    border-radius: 5px;"                 // 圆角半径
			"    color: white;"                        // 字体颜色
			"    padding: 10px;"                       // 内边距
			"}"
			"QPushButton:hover {"
			"    background-color: rgb(42, 156, 112);" // 鼠标悬停颜色
			"}"
			"QPushButton:pressed {"   
			"    background-color: rgb(42, 156, 112);" // 鼠标点击颜色
			"}"
		);
	}
	builder.endLayout();

	CustomSlider* quantitySlider = new CustomSlider(this);
	builder.addWidget(quantitySlider);
	builder.addSplitLine();
}
