javascript: (
  followers = [{ username: '', full_name: '' }],
  followings = [{ username: '', full_name: '' }],
  await(async () => {
    try {
      if(window.location.hostname != 'www.instagram.com') throw new Error('Please do this on www.instagram.com');
      const username = prompt('※按下確定後等待5~10秒，或開啟console確認進度。\n\n請輸入ID:');
      if (username === null) throw new Error('Action cancelled.');
      followers = [];
      followings = [];
      const btn_copy = document.body.appendChild(document.createElement('button'));
      // 設置按鈕的文字和樣式
      Object.assign(
        Object.assign(btn_copy, {
          innerText: 'Copy',
          disabled: true,
          onclick: () => {
            try {
              navigator.clipboard.writeText(JSON.stringify({ followers, followings }));
              console.log(`${followers.length} followers and ${followings.length} followings have been copied to clipboard.`);
            } catch (err) {
              alert(err);
              console.log('btn_copy.onclick failed with error.');
            }
          }
        }).
        style, {
          position: 'fixed', // 固定位置
          bottom: '20px', // 距離底部 20 像素
          right: '65px', // 距離右邊 20 像素
          padding: '10px 20px', // 添加內邊距
          fontSize: '16px', // 字體大小
          backgroundColor: '#CCCCCC', // 背景顏色(未啟用) => #007BFF
          color: '#FFFFFF', // 字體顏色 
          border: 'none', // 無邊框
          borderRadius: '5px', // 圓角
          cursor: 'pointer' // 鼠標指針樣式
        }
      )

      const btn_close = document.body.appendChild(btn_copy.cloneNode(false));
      Object.assign(
        Object.assign(btn_close, {
          innerText: '✕',
          disabled: false,
          onclick: () => {
            btn_copy.remove();
            btn_close.remove();
            console.log('UI closed.')
          }
        }).
        style, {
          right: '20px',
          padding: '8px',
          backgroundColor: '#f87daa',
          width: '38px',
          height: '38px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          fontWeight: 700,
        }
      )
      console.log(`Process started! Give it a couple of seconds`);
      const userQueryRes = await fetch(`https://www.instagram.com/web/search/topsearch/?query=${username}`);
      const userQueryJson = await userQueryRes.json();
      const userId = userQueryJson.users.map(u => u.user).filter(u => u.username === username)[0].pk;
      let after = null;
      let has_next = true;
      console.log(`Fetching follower list...`);
      while (has_next) {
        await fetch(`https://www.instagram.com/graphql/query/?query_hash=c76146de99bb02f6415203be841dd25a&variables=` +
          encodeURIComponent(JSON.stringify({
            id: userId,
            include_reel: true,
            fetch_mutual: true,
            first: 50,
            after: after,
          }))
        )
          .then((res) => res.json())
          .then((res) => {
            has_next = res.data.user.edge_followed_by.page_info.has_next_page;
            after = res.data.user.edge_followed_by.page_info.end_cursor;
            followers = followers.concat(res.data.user.edge_followed_by.edges.map(({ node }) => {
              return { username: node.username, full_name: node.full_name, };
            }));
          });
      }
      console.log(`Done.`);
      after = null;
      has_next = true;
      console.log(`Fetching following list...`);
      while (has_next) {
        await fetch(`https://www.instagram.com/graphql/query/?query_hash=d04b0a864b4b54837c0d870b0e77e076&variables=` +
          encodeURIComponent(JSON.stringify({
            id: userId, include_reel: true,
            fetch_mutual: true,
            first: 50,
            after: after,
          }))
        )
          .then((res) => res.json())
          .then((res) => {
            has_next = res.data.user.edge_follow.page_info.has_next_page;
            after = res.data.user.edge_follow.page_info.end_cursor;
            followings = followings.concat(
              res.data.user.edge_follow.edges.map(({ node }) => {
                return {
                  username: node.username,
                  full_name: node.full_name,
                };
              })
            );
          });
      }
      console.log(`Done.`);
      console.log('Process ended.');
      btn_copy.style.backgroundColor = '#007BFF';
      btn_copy.disabled = false;
    } catch (error) {
      alert(error);
      console.log('Process stopped by error.');
      return '0';
    }
  })()
)