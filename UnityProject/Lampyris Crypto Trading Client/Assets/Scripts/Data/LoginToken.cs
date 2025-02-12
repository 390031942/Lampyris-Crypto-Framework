/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Client.
 * File created at 2024-12-04
 */

using System.Collections.Generic;

namespace HongJinInvestment.OKX.Client.Data
{
    public class LoginToken
    {
        // Token 名称，方便区分
        public string name;
        
        // API KEY，由OKX平台随机提供
        public string key;
        
        // API SECRET KEY, 由OKX平台随机提供
        public string secretKey;
        
        // Token API密码，由OKX官网用户自行设置
        public string passPhrase;
    }

    public class LoginHistoryDataObject
    {
        public List<LoginToken> loginTokenList = new List<LoginToken>();
    }
}