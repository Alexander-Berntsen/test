import React, { Component } from 'react';

class CheckOutComponent extends Component {
  constructor(props) {
    super(props);
    this.checkOut = this.checkOut.bind(this);
    this.navigateToCamera = this.navigateToCamera.bind(this);
  }

  checkOut() {
    fetch('https://jsonplaceholder.typicode.com/posts', {
      method: 'POST',
      body: JSON.stringify(this.state),
      headers: {
        'Content-type': 'application/json; charset=UTF-8'
      }
    })
      .then(response => response.json())
      .catch(error => console.error('Error:', error))
      .then(response => {
        console.log('Success:', response);
        alert('Checkout successful');
        this.navigateToCamera();
      });
  }

  navigateToCamera() {
    this.props.history.replace('/camera');
  }

  render() {
    return (
      <div>
        <button onClick={this.checkOut} style={styles.button}>
          Checkout
        </button>
        <button onClick={this.navigateToCamera} style={styles.button}>
          New check-in
        </button>
      </div>
    );
  }
}

const styles = {
  text: {
    margin: 'auto',
    display: 'block'
  },
  button: {
    margin: 'auto',
    display: 'block'
  }
};

export default CheckedInComponent;
